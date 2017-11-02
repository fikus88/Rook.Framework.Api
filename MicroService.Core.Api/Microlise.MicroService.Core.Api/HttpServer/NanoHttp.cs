using Microlise.MicroService.Core.Common;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace Microlise.MicroService.Core.Api.HttpServer
{
	public class NanoHttp : INanoHttp
	{
		private TcpListener listener;
		private Task allocator;
		private readonly TaskFactory processorFactory = new TaskFactory();
		private readonly IRequestBroker requestBroker;
		private readonly ILogger logger;
		private readonly bool requiresAuthorisation;
		private readonly int port;
		private readonly int backlog;
		private readonly int requestTimeout;

		public NanoHttp(IRequestBroker requestBroker, IConfigurationManager configurationManager, ILogger logger)
		{
			this.requestBroker = requestBroker;
			this.logger = logger;

			requiresAuthorisation = string.Equals(configurationManager.AppSettings["RequiresAuthorisation"], "true", StringComparison.OrdinalIgnoreCase);

			if (!int.TryParse(configurationManager.AppSettings["Port"], out port))
				port = 80;

			if (!int.TryParse(configurationManager.AppSettings["Backlog"], out backlog))
				backlog = 16;

			if (!int.TryParse(configurationManager.AppSettings["RequestTimeout"], out requestTimeout))
				requestTimeout = 500;

		}

		public void Start()
		{
			listener = new TcpListener(new IPEndPoint(IPAddress.Any, port));
			listener.Start(backlog);

			logger.Info($"{nameof(NanoHttp)}.{nameof(Start)}", new LogItem("Event", "Listener started"), new LogItem("Port", port), new LogItem("Backlog", backlog));

			allocator = new Task(AllocationMain);
			allocator.Start();
		}

		private void AllocationMain()
		{
			while (true)
			{
				while (!listener.Pending()) { Thread.Sleep(1); }
				Socket s = listener.AcceptSocketAsync().Result;
				logger.Trace($"{nameof(NanoHttp)}.{nameof(AllocationMain)}", new LogItem("Event", "Accepted socket"), new LogItem("Client", s.RemoteEndPoint.ToString));
				processorFactory.StartNew(() => Processor(s));
			}
		}

		private void Processor(Socket s)
		{
			byte[] buffer = new byte[4096];
			byte[] content = null;
			int contentOffset = 0;
			HttpRequest request = null;

			Stopwatch connectionTimer = Stopwatch.StartNew();

			while (true)
			{
				while (s.Available == 0 && connectionTimer.ElapsedMilliseconds < requestTimeout) Thread.Sleep(1);

				if (s.Available == 0)
				{
					s.Shutdown(SocketShutdown.Both);
					s.Dispose();
					logger.Trace($"{nameof(NanoHttp)}.{nameof(Processor)}", new LogItem("Event", "Closed socket"), new LogItem("Reason", $"No request received in {requestTimeout}ms"));
					return;
				}

				int bytesReceived = s.Receive(buffer);
				byte[] received = new byte[bytesReceived];
				Array.Copy(buffer, 0, received, 0, bytesReceived);
				if (request == null)
				{
					int i;
					if ((i = received.FindPattern((byte)13, (byte)10, (byte)13, (byte)10)) > 0)
					{
					    try
					    {
					        request = new HttpRequest(received.SubArray(i), requiresAuthorisation);
					    }
					    catch (SecurityTokenException ex)
					    {
					        s.Shutdown(SocketShutdown.Both);
					        s.Dispose();
					        logger.Trace($"{nameof(NanoHttp)}.{nameof(Processor)}", new LogItem("Event", "Closed socket"), new LogItem("Reason", $"Authorisation required, but invalid token supplied ({ex.GetType()})"));
					        return;
                        }
					    logger.Trace($"{nameof(NanoHttp)}.{nameof(Processor)}", new LogItem("Event", "Received request"), new LogItem("Verb", request.Verb.ToString), new LogItem("Path", request.Path));
						if (request.RequestHeader.ContainsKey("Content-Length"))
						{
							content = new byte[int.Parse(request.RequestHeader["Content-Length"])];
							Array.Copy(received, i, content, 0, bytesReceived - i);
							contentOffset = bytesReceived - i;
						}
						else
							content = new byte[0];

						if (requiresAuthorisation && request.SecurityToken == null)
						{
							s.Shutdown(SocketShutdown.Both);
							s.Dispose();
							logger.Trace($"{nameof(NanoHttp)}.{nameof(Processor)}", new LogItem("Event", "Closed socket"), new LogItem("Reason", "Authorisation required, but no token provided"));
							return;
						}
						received = new byte[0];
						bytesReceived = 0;
					}
				}
				if (request != null)
				{
					Array.Copy(received, 0, content, contentOffset, bytesReceived);
					contentOffset += bytesReceived;
					if (contentOffset >= content.Length - 1)
					{
						request.Body = content;
						logger.Trace($"{nameof(NanoHttp)}.{nameof(Processor)}", new LogItem("Event", "HandleRequest started"));
						Stopwatch responseTimer = Stopwatch.StartNew();
						HttpResponse response = requestBroker.HandleRequest(request);
						logger.Trace($"{nameof(NanoHttp)}.{nameof(Processor)}", new LogItem("Event", "HandleRequest completed"), new LogItem("DurationMilliseconds", responseTimer.Elapsed.TotalMilliseconds));

						response.Headers.Add("Access-Control-Allow-Origin", "*");
						response.Headers.Add("Access-Control-Allow-Methods", "POST,GET,DELETE,PUT");
						response.Headers.Add("Access-Control-Allow-Headers", "authorization");

						s.Send(response.ToByteArray());
						s.Shutdown(SocketShutdown.Both);
						s.Dispose();
						logger.Trace($"{nameof(NanoHttp)}.{nameof(Processor)}", new LogItem("Event", "Closed socket"), new LogItem("Reason", "Response complete"));
						return;
					}
				}
			}
		}
	}
}
