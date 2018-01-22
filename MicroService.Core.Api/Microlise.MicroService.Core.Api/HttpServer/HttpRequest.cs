using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Microlise.MicroService.Core.Api.Utils;
using Microlise.MicroService.Core.Common;
using Microlise.MicroService.Core.IoC;
using Microsoft.IdentityModel.Tokens;

namespace Microlise.MicroService.Core.Api.HttpServer
{
    public interface IHttpRequest
    {
        Uri Uri { get; }
        HttpVerb Verb { get; }
        string Path { get; }
        string HttpVersion { get; }
        CaseInsensitiveDictionary RequestHeader { get; }
        JwtSecurityToken SecurityToken { get; }
        byte[] Body { get; }
        AutoDictionary<string, string> Parameters { get; }
        void SetUriPattern(string value);
    }

    public class HttpRequest : IHttpRequest
    {
        public Uri Uri { get; }

        public HttpVerb Verb { get; }
        public string Path { get; }
        public string HttpVersion { get; }
        public CaseInsensitiveDictionary RequestHeader { get; } = new CaseInsensitiveDictionary();
        public JwtSecurityToken SecurityToken { get; }
        public byte[] Body { get; internal set; }
        public AutoDictionary<string, string> Parameters { get; private set; }

        private string uriPattern;

        public void SetUriPattern(string value)
        {
            uriPattern = value;
            if (Parameters == null && uriPattern != null)
            {
                string[] pathParts = Path.Split('?');

                // parse UriPattern
                // UriPattern will be like:
                // "/rest/{version}/driver/{driverId}"
                string[] tokens = uriPattern.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                string[] values = pathParts[0].Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                if (tokens.Length != values.Length)
                    throw new InvalidOperationException("WTF");

                Parameters = new AutoDictionary<string, string>();

                for (int i = 0; i < tokens.Length; i++)
                {
                    string token = tokens[i];
                    if (token.StartsWith("{") && token.EndsWith("}"))
                    {
                        string key = token.Trim('{', '}');
                        Parameters.Add(key, Uri.UnescapeDataString(values[i]));
                    }
                }

                // parse Get params
                if (pathParts.Length > 1)
                {
                    string paramsString = pathParts[1];
                    string[] parameters = paramsString.Split('&');
                    foreach (string parameter in parameters)
                    {
                        string[] parts = parameter.Split('=');
                        if (parts.Length > 1) Parameters.Add(parts[0], Uri.UnescapeDataString(parts[1]));
                    }
                }
            }
        }

        private readonly JwtSecurityTokenHandler securityTokenHandler = new JwtSecurityTokenHandler();

        private static TokenValidationParameters TokenValidationParameters { get; } = new TokenValidationParameters
        {
            IssuerSigningKeys = GetSigningKeys(),
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateActor=false
        };

        public HttpRequest(byte[] headerBytes, bool authorisationRequired)
        {
            string data = Encoding.ASCII.GetString(headerBytes);

            // construct header from http data
            string[] datas = data.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            // First line
            string[] parts = datas[0].Split(' ');

            Verb = (HttpVerb)Enum.Parse(typeof(HttpVerb), parts[0], true);
            Path = parts[1];
            HttpVersion = parts[2];

            // Subsequent lines
            for (int i = 1; i < datas.Length; i++)
            {
                string key = datas[i].Substring(0, datas[i].IndexOf(':'));
                string value = datas[i].Substring(datas[i].IndexOf(' ') + 1);
                RequestHeader.Add(key, value);
            }

            // Uri
            Uri = new Uri("http://" + RequestHeader["Host"] + Path);

            // decode JWT
            if (authorisationRequired && RequestHeader.ContainsKey("Authorization") && RequestHeader["Authorization"].StartsWith("Bearer "))
            {
                string payload = RequestHeader["Authorization"].Substring(7);
                securityTokenHandler.ValidateToken(payload, TokenValidationParameters,
                    out SecurityToken token);

                SecurityToken = (JwtSecurityToken)token;
            }
        }

        private static IEnumerable<SecurityKey> GetSigningKeys()
        {
            IConfigurationManager configurationManager = Container.GetInstance<IConfigurationManager>();
            List<byte> longBuffer = new List<byte>();
            using (Socket socket = new Socket(SocketType.Stream, ProtocolType.IP) { ReceiveBufferSize = 8192, LingerState = new LingerOption(false, 0), NoDelay = false })
            {
                socket.Connect(configurationManager.AppSettings["IdentityServerAddress"], UInt16.Parse(configurationManager.AppSettings["IdentityServerPort"]));
                // Use HTTP/0.9, then we don't need to include a framework for this one little requirement
                socket.Send(Encoding.ASCII.GetBytes($"GET {configurationManager.AppSettings["IdentityServerPath"]}/.well-known/jwks\r\n"));
                Stopwatch w = Stopwatch.StartNew();
                while (socket.Connected && w.ElapsedMilliseconds < 100)
                {
                    if (socket.Available > 0)
                    {
                        byte[] buffer = new byte[socket.Available];
                        socket.Receive(buffer);
                        longBuffer.AddRange(buffer);
                        w.Restart();
                    }
                    else
                    {
                        Thread.Sleep(10);
                    }
                }

                string keys = Encoding.ASCII.GetString(longBuffer.ToArray());

                JsonWebKeySet keyset = new JsonWebKeySet(keys);
                return keyset.GetSigningKeys();
            }
        }
    }
}
