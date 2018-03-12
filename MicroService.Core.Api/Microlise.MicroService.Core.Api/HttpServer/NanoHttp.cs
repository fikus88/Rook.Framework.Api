using Microlise.MicroService.Core.Common;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microlise.MicroService.Core.IoC;
using Microsoft.IdentityModel.Tokens;

namespace Microlise.MicroService.Core.Api.HttpServer
{
    public class NanoHttp : INanoHttp
    {
        private TcpListener listener;
        private TcpListener tlsListener;
        private Task allocator;
        private readonly IRequestBroker requestBroker;
        private readonly ILogger logger;
        private readonly bool validJwtRequired;
        private readonly int port;
        private readonly int backlog;
        private readonly int requestTimeout;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private CancellationToken allocationCancellationToken;
        private readonly int tlsPort;
        private readonly X509Certificate2 myCert;

        public NanoHttp(IRequestBroker requestBroker, IConfigurationManager configurationManager, ILogger logger)
        {
            this.requestBroker = requestBroker;
            this.logger = logger;

            validJwtRequired = string.Equals(configurationManager.AppSettings["RequiresJwtValidation"], "true", StringComparison.OrdinalIgnoreCase);

            if (!int.TryParse(configurationManager.AppSettings["Port"], out port))
                port = 80;

            if (!int.TryParse(configurationManager.AppSettings["TlsPort"], out tlsPort))
                tlsPort = -1;

            if (!int.TryParse(configurationManager.AppSettings["Backlog"], out backlog))
                backlog = 16;

            if (!int.TryParse(configurationManager.AppSettings["RequestTimeout"], out requestTimeout))
                requestTimeout = 500;

            if (configurationManager.AppSettings.ContainsKey("CertificateLocation"))
                myCert = new X509Certificate2(File.ReadAllBytes(configurationManager.AppSettings["CertificateLocation"]));
        }

        public void Start()
        {
            listener = new TcpListener(new IPEndPoint(IPAddress.Any, port));
            listener.Start((int)(backlog * (tlsPort > 0 ? 0.5 : 1)));

            if (tlsPort > 0)
            {
                tlsListener = new TcpListener(new IPEndPoint(IPAddress.Any, tlsPort));
                tlsListener.Start((int)(backlog * 0.5));
            }

            logger.Info($"{nameof(NanoHttp)}.{nameof(Start)}", new LogItem("Event", "Listener started"), new LogItem("Port", port), new LogItem("Backlog", backlog));

            allocationCancellationToken = cts.Token;
            allocator = new Task(AllocationMain, allocationCancellationToken);
            allocator.Start();
        }

        public void Stop()
        {
            cts.Cancel();
            allocator = null;
        }

        private void AllocationMain(object cancellationToken)
        {
            try
            {
                while (true)
                {
                    while (!listener.Pending() || (tlsListener != null && !tlsListener.Pending()))
                    {
                        ((CancellationToken)cancellationToken).ThrowIfCancellationRequested();
                        Thread.Sleep(1);
                    }
                    Socket s = listener.Pending() ? listener.AcceptSocketAsync().Result : tlsListener?.AcceptSocketAsync().Result;
                    logger.Debug($"{nameof(NanoHttp)}.{nameof(AllocationMain)}", new LogItem("Event", "Accepted socket"),
                        new LogItem("Client", s.RemoteEndPoint.ToString));
                    Task.Run(() => Processor(s), allocationCancellationToken);
                    logger.Debug($"{nameof(NanoHttp)}.{nameof(AllocationMain)}", new LogItem("Event", "Processor started"),
                        new LogItem("Client", s.RemoteEndPoint.ToString));
                }
            }
            catch (OperationCanceledException)
            {
                listener.Stop();
                listener.Server.Dispose();
            }
        }

        private void Processor(Socket s)
        {
            byte[] buffer = new byte[4096];
            byte[] content = null;
            int contentOffset = 0;
            HttpRequest request = null;

            Stopwatch connectionTimer = Stopwatch.StartNew();

            using (NetworkStream ns = new NetworkStream(s))
            {
                Stream dataStream = ns;
                bool tlsConnection = ((IPEndPoint)s.LocalEndPoint).Port == tlsPort;
                if (tlsConnection)
                {
                    dataStream = new SslStream(dataStream);

                    ((SslStream)dataStream).AuthenticateAsServerAsync(myCert, false,
                        SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12, false).Wait(100);
                }

                while (true)
                {
                    int bytesReceived = dataStream.Read(buffer, 0, buffer.Length);// s.Receive(buffer);
                    while (bytesReceived == 0 && connectionTimer.ElapsedMilliseconds < requestTimeout) Thread.Sleep(1);

                    if (bytesReceived == 0)
                    {
                        s.Shutdown(SocketShutdown.Both);
                        s.Dispose();
                        dataStream.Dispose();
                        logger.Trace($"{nameof(NanoHttp)}.{nameof(Processor)}", new LogItem("Event", "Closed socket"), new LogItem("Reason", $"No request received in {requestTimeout}ms"));
                        return;
                    }


                    byte[] received = new byte[bytesReceived];
                    Array.Copy(buffer, 0, received, 0, bytesReceived);
#if DEBUG
                    string receivedText = Encoding.ASCII.GetString(received);
#endif

                    if (request == null)
                    {
                        int i = received.FindPattern((byte)13, (byte)10, (byte)13, (byte)10);

                        // If we have a double CRLF then we have a complete header, otherwise keep looping
                        if (i == -1) continue;

                        request = ParseHeader(i, ref received, ref content, ref contentOffset, ref bytesReceived);

                        if (request == null)
                        {
                            s.Shutdown(SocketShutdown.Both);
                            s.Dispose();
                            dataStream.Dispose();
                            return;
                        }

                        if (tlsConnection)
                        {
                            request.ServerCertificate = ((SslStream)dataStream).LocalCertificate;
                            request.ClientCertificate = ((SslStream)dataStream).RemoteCertificate;
                        }
                    }

                    Array.Copy(received, 0, content, contentOffset, bytesReceived);
                    contentOffset += bytesReceived;
                    if (contentOffset < content.Length - 1) continue;

                    // Completed loading body, which could have urlencoded content :(
                    TokenState tokenState = request.FinaliseLoad(validJwtRequired);

                    if (tokenState == TokenState.Invalid)
                    {
                        logger.Trace($"{nameof(NanoHttp)}.{nameof(Processor)}", new LogItem("Event", "Closed socket"),
                            new LogItem("Reason", "Valid token required, but invalid token provided (or missing)"));
                        s.Shutdown(SocketShutdown.Both);
                        s.Dispose();
                        dataStream.Dispose();
                        return;
                    }

                    request.Body = content;
                    logger.Trace($"{nameof(NanoHttp)}.{nameof(Processor)}", new LogItem("Event", "HandleRequest started"));
                    Stopwatch responseTimer = Stopwatch.StartNew();
                    IHttpResponse response;
                    if (tokenState == TokenState.Expired || tokenState == TokenState.NotYetValid)
                    {
                        response = Container.GetNewInstance<IHttpResponse>();
                        response.HttpStatusCode = HttpStatusCode.Unauthorized;
                    }
                    else
                    {
                        response = requestBroker.HandleRequest(request);
                        logger.Trace($"{nameof(NanoHttp)}.{nameof(Processor)}",
                            new LogItem("Event", "HandleRequest completed"),
                            new LogItem("DurationMilliseconds", responseTimer.Elapsed.TotalMilliseconds));
                    }

                    response.Headers.Add("Access-Control-Allow-Origin", "*");
                    response.Headers.Add("Access-Control-Allow-Methods", "POST,GET,DELETE,PUT,OPTIONS");
                    response.Headers.Add("Access-Control-Allow-Headers", "authorization");

                    byte[] outBuffer = response.ToByteArray();
                    dataStream.Write(outBuffer, 0, outBuffer.Length);
                    dataStream.Flush();
                    s.Shutdown(SocketShutdown.Both);
                    s.Dispose();
                    dataStream.Dispose();
                    logger.Trace($"{nameof(NanoHttp)}.{nameof(Processor)}", new LogItem("Event", "Closed socket"), new LogItem("Reason", "Response complete"));
                    return;
                }
            }
        }

        private HttpRequest ParseHeader(int i, ref byte[] received, ref byte[] content, ref int contentOffset, ref int bytesReceived)
        {
            HttpRequest request;
            try
            {
                i += 1;
                request = new HttpRequest(received.SubArray(i));
            }
            catch (SecurityTokenException ex)
            {
                logger.Trace($"{nameof(NanoHttp)}.{nameof(Processor)}", new LogItem("Event", "Closed socket"),
                    new LogItem("Reason", $"Authorisation required, but invalid token supplied ({ex.GetType()})"));
                return null;
            }
            logger.Trace($"{nameof(NanoHttp)}.{nameof(Processor)}", new LogItem("Event", "Received request"),
                new LogItem("Verb", request.Verb.ToString), new LogItem("Path", request.Path));
            if (request.RequestHeader.ContainsKey("Content-Length"))
            {
                int contentLength = int.Parse(request.RequestHeader["Content-Length"]);
                content = new byte[contentLength];
                i += 4;
                Array.Copy(received, i, content, 0, Math.Min(received.Length - i, contentLength));
                contentOffset += received.Length - i;
            }
            else
                content = new byte[0];


            received = new byte[0];
            bytesReceived = 0;
            return request;
        }
    }
}
