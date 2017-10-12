using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MicroService.Core.Api.HttpServer
{
	public class NanoHttp : INanoHttp
	{
		TcpListener listener;
		Task allocator;
		readonly TaskFactory processorFactory = new TaskFactory();
		private readonly IRequestBroker requestBroker;

		public NanoHttp(IRequestBroker requestBroker)
		{
			this.requestBroker = requestBroker;
		}

		public void Start()
		{
			listener = new TcpListener(new IPEndPoint(IPAddress.Any, 80));
			listener.Start(16);

			allocator = new Task(AllocationMain);
			allocator.Start();
			Console.WriteLine("NanoHttp started");
		}

		private void AllocationMain()
		{
			while (true)
			{
				while (!listener.Pending()) { Thread.Sleep(1); }
				Console.Write("Incoming connection...");
				Socket s = listener.AcceptSocketAsync().Result;
				Console.WriteLine(" ... from " + ((IPEndPoint) s.RemoteEndPoint).Address);
				processorFactory.StartNew(() => Processor(s));
			}
		}

		private void Processor(Socket s)
		{
			byte[] buffer = new byte[4096];
			byte[] content = null;
			int contentOffset = 0;
			HttpRequest request = null;

			while (true)
			{
				while (s.Available == 0) Thread.Sleep(10);

				int bytesReceived = s.Receive(buffer);
				byte[] received = new byte[bytesReceived];
				Array.Copy(buffer, 0, received, 0, bytesReceived);
				if (request == null)
				{
					Console.WriteLine(Encoding.ASCII.GetString(received));

					int i;
					if ((i = received.FindPattern((byte) 13, (byte) 10, (byte) 13, (byte) 10)) > 0)
					{
						request = new HttpRequest(received.SubArray(i));
						if (request.RequestHeader.ContainsKey("Content-Length"))
						{
							content = new byte[int.Parse(request.RequestHeader["Content-Length"])];
							Array.Copy(received, i, content, 0, bytesReceived - i);
							contentOffset = bytesReceived - i;
						}
						else
							content = new byte[0];

						if (request.SecurityToken != null)
						{
							s.Shutdown(SocketShutdown.Both);
							s.Dispose();
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
						HttpResponse response = requestBroker.HandleRequest(request);

						response.Headers.Add("Access-Control-Allow-Origin", "*");
						response.Headers.Add("Access-Control-Allow-Methods", "POST,GET,DELETE,PUT");
						response.Headers.Add("Access-Control-Allow-Headers", "authorization");

						s.Send(response.ToByteArray());
						s.Shutdown(SocketShutdown.Both);
						s.Dispose();
						return;
					}
				}
			}
		}
	}
}
