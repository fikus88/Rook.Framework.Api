using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microlise.MicroService.Core.Common;

namespace Microlise.MicroService.Core.Api.HttpServer
{
	public class HttpRequest
	{
		public Uri Uri { get; }

		public HttpVerb Verb { get; }
		public string Path { get; }
		public string HttpVersion { get; }
		public AutoDictionary<string, string> RequestHeader { get; } = new AutoDictionary<string, string>();
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

		public HttpRequest(byte[] headerBytes)
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
			if (RequestHeader.ContainsKey("Authorization") && RequestHeader["Authorization"].StartsWith("Bearer "))
			{
				string payload = RequestHeader["Authorization"].Substring(7);
				SecurityToken = new JwtSecurityToken(payload);
			}
		}
	}
}
