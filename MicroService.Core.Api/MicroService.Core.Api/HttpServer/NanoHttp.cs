using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MicroService.Core.Api.HttpServer
{
	internal class NanoHttp : INanoHttp
	{
		Socket listener;
		Task allocator;
		TaskFactory processorFactory = new TaskFactory();
		private readonly IRequestBroker requestBroker;

		public NanoHttp(IRequestBroker requestBroker)
		{
			this.requestBroker = requestBroker;
		}

		public void Start()
		{
			listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			allocator = new Task(AllocationMain);
			allocator.Start();
		}

		private void AllocationMain()
		{
			while (true)
			{
				Socket s = listener.Accept();
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
				if (s.Available > 0)
				{
					int bytesReceived = s.Receive(buffer);
					byte[] received = new byte[bytesReceived];
					Array.Copy(buffer, 0, received, 0, bytesReceived);
					if (request == null)
					{
						int i;
						if ((i = received.FindPattern((byte)13, (byte)10, (byte)13, (byte)10)) > 0)
						{
							request = new HttpRequest(received.SubArray(i));
							content = new byte[int.Parse(request.RequestHeader["Content-Length"])];
							Array.Copy(received, i, content, 0, bytesReceived - i);
							contentOffset = bytesReceived - i;
							if (request.SecurityToken == null)
							{
								s.Shutdown(SocketShutdown.Both);
								s.Dispose();
								return;
							}
						}
					}
					if (request != null)
					{
						Array.Copy(received, 0, content, contentOffset, bytesReceived);
						contentOffset += bytesReceived;
						if (contentOffset == content.Length - 1)
						{
							request.Body = content;
							HttpResponse response = requestBroker.HandleRequest(request);

							response.Headers.Add("Access-Control-Allow-Origin", "*");
							response.Headers.Add("Access-Control-Allow-Methods", "POST,GET,DELETE,PUT");
							response.Headers.Add("Access-Control-Allow-Headers", "authorization");

							s.Send(response.ToByteArray());
						}
					}
				}
			}
		}
	}
}
