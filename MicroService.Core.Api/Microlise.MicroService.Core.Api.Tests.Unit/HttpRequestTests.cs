using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Microlise.MicroService.Core.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microlise.MicroService.Core.Api.HttpServer;
using Microlise.MicroService.Core.Common;
using Moq;

namespace Microlise.MicroService.Core.Api.Tests.Unit
{
    [TestClass]
    public class HttpRequestTests
    {
        private string TestRequest = "GET /description HTTP/1.1\r\nHost: localhost\r\nContent-Length: 123\r\n\r\n012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012";
        private string TestRequestHugeContent = "GET /description HTTP/1.1\r\nHost: localhost\r\nContent-Length: 2000\r\n\r\n01234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789";
        private string TestRequestLowerCaseHeaders = "GET /description HTTP/1.1\r\nhost: localhost\r\ncontent-length: 123\r\n\r\n012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012";

        private string MandeepTest = "GET /users/authenticate HTTP/1.1\r\nAuthorization: Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6IldIUk9naXVjX24zWmIzaV81cTdkN2M1Y2I0cyIsImtpZCI6IldIUk9naXVjX24zWmIzaV81cTdkN2M1Y2I0cyJ9.eyJpc3MiOiJodHRwczovL2xvY2FsaG9zdC9pZGVudGl0eVNlcnZpY2UvaWRlbnRpdHkiLCJhdWQiOiJodHRwczovL2xvY2FsaG9zdC9pZGVudGl0eVNlcnZpY2UvaWRlbnRpdHkvcmVzb3VyY2VzIiwiZXhwIjoxNTE2MTk3Mjc3LCJuYmYiOjE1MTYxOTM2NzcsImNsaWVudF9pZCI6ImNsaWVudHJlc291cmNlb3duZXJmbG93Iiwic2NvcGUiOlsiaWRlbnRpdHlzZXJ2aWNlYXBpIiwib3BlbmlkIl0sInN1YiI6Im1zaW5naCIsImF1dGhfdGltZSI6MTUxNjE5MzY3NywiaWRwIjoiaWRzcnYiLCJuYW1lIjoibXNpbmdoIiwiZ2l2ZW5fbmFtZSI6Ik1hbmRlZXAiLCJmYW1pbHlfbmFtZSI6IlNpbmdoIiwiZW1haWwiOiJtYW5kZWVwLnNpbmdoQG1pY3JvbGlzZS5jb20iLCJyb2xlIjoiW3tcIlJvbGVJZFwiOlwiMVwiLFwiTmFtZVwiOlwiQWRtaW5cIixcIkRlc2NyaXB0aW9uXCI6XCJjYW4gcGVyZnJvbSBhbGwgdGhlIG9wZXJhdGlvblwifV0iLCJvcmdhbmlzYXRpb25pZHMiOiJbXCJNSUMwMDFcIl0iLCJhbXIiOlsicGFzc3dvcmQiXX0.fw85A6brNrm5Wc_r54AsnhnZCzUFkNgGZvSDssJPCbDWwYCiUL7vLshKoRUYb8u0vBohWna-BYWaCvD8-5ku--6N1aHPCQm7ii-McHYfeKnEd1_qAQKFa0IGreClYZhLtsLwBUzLDvTzWNt103WDj592BQQSt550RdPqZyv_SB8eWnmmAYkq-EMOIqHHM_ZtU78M7Lw3yjdKdqRjGDWtVU52CyQ_br95HphHukk4vd-7bAocNpFqP07d0bZ5j-Efca-0bzQtVzmzal_cVWwvQHZHxHbIjAUW_2yHQlB2ROdOfDYoICNKcIdGP1Kf5ysFONsCaygHJo-xHCLsMYPmPA\r\ncache-control: no-cache\r\nPostman-Token: f8dee338-d26f-4966-a7fe-80c6370cc807\r\nUser-Agent: PostmanRuntime/7.1.1\r\nAccept: */*\r\nHost: 172.17.157.74:81\r\naccept-encoding: gzip, deflate\r\nConnection: keep-alive\r\n\r\n";

        [TestMethod]
        public void TestMandeepsRequest()
        {
            MockRequestBroker broker = new MockRequestBroker();

            Mock<IConfigurationManager> config = new Mock<IConfigurationManager>();
            config.Setup(conf => conf.AppSettings["Port"]).Returns("8884");
            Mock<ILogger> logger = new Mock<ILogger>();
            NanoHttp nanoHttp = new NanoHttp(broker, config.Object, logger.Object);
            try
            {
                nanoHttp.Start();
                Socket client = new Socket(SocketType.Stream, ProtocolType.IP)
                {
                    ReceiveBufferSize = 8192,
                    LingerState = new LingerOption(false, 0),
                    NoDelay = false
                };
                client.Connect("127.0.0.1", 8884);

                client.Send(Encoding.ASCII.GetBytes(MandeepTest));
                broker.WaitHandle.WaitOne();

                client.Shutdown(SocketShutdown.Both);
                client.Dispose();
            }
            finally
            {
                nanoHttp.Stop();
            }

            Assert.IsNotNull(broker.ReceivedRequest);
            Assert.AreEqual(HttpVerb.Get, broker.Verb);
            Assert.AreEqual("/users/authenticate", broker.Path);
            Assert.AreEqual(0, broker.Body.Length);            
        }

        [TestMethod]
        public void TestProcessingGetRequestWithProperCaseHeaders()
        {
            MockRequestBroker broker = new MockRequestBroker();

            Mock<IConfigurationManager> config = new Mock<IConfigurationManager>();
            config.Setup(conf => conf.AppSettings["Port"]).Returns("8880");
            Mock<ILogger> logger = new Mock<ILogger>();
            NanoHttp nanoHttp = new NanoHttp(broker, config.Object, logger.Object);
            try
            {
                nanoHttp.Start();
                Socket client = new Socket(SocketType.Stream, ProtocolType.IP)
                {
                    ReceiveBufferSize = 8192,
                    LingerState = new LingerOption(false, 0),
                    NoDelay = false
                };
                client.Connect("127.0.0.1", 8880);

                client.Send(Encoding.ASCII.GetBytes(TestRequest));
                broker.WaitHandle.WaitOne();

                client.Shutdown(SocketShutdown.Both);
                client.Dispose();
            }
            finally
            {
                nanoHttp.Stop();
            }

            Assert.AreEqual(HttpVerb.Get, broker.Verb);
            Assert.AreEqual(123, broker.Body.Length);
            Assert.AreEqual(48, broker.Body[0]);
            Assert.AreEqual(50, broker.Body[122]);
        }

        [TestMethod]
        public void TestProcessingGetRequestWithLowerCaseHeaders()
        {
            MockRequestBroker broker = new MockRequestBroker();

            Mock<IConfigurationManager> config = new Mock<IConfigurationManager>();
            int port = 8881;
            config.Setup(conf => conf.AppSettings["Port"]).Returns(port.ToString);
            Mock<ILogger> logger = new Mock<ILogger>();
            NanoHttp nanoHttp = new NanoHttp(broker, config.Object, logger.Object);
            try
            {
                nanoHttp.Start();
                Socket client = new Socket(SocketType.Stream, ProtocolType.IP)
                {
                    ReceiveBufferSize = 8192,
                    LingerState = new LingerOption(false, 0),
                    NoDelay = false
                };
                client.Connect("127.0.0.1", port);

                client.Send(Encoding.ASCII.GetBytes(TestRequestLowerCaseHeaders));
                broker.WaitHandle.WaitOne();

                client.Shutdown(SocketShutdown.Both);
                client.Dispose();
            }
            finally
            {
                nanoHttp.Stop();
            }
            Assert.AreEqual(HttpVerb.Get, broker.Verb);
            Assert.AreEqual(123, broker.Body.Length);
            Assert.AreEqual(48, broker.Body[0]);
            Assert.AreEqual(50, broker.Body[122]);
        }

        [TestMethod]
        public void TestProcessingGetRequestWithHugeContent()
        {
            MockRequestBroker broker = new MockRequestBroker();

            Mock<IConfigurationManager> config = new Mock<IConfigurationManager>();
            int port = 8882;
            config.Setup(conf => conf.AppSettings["Port"]).Returns(port.ToString);
            Mock<ILogger> logger = new Mock<ILogger>();
            NanoHttp nanoHttp = new NanoHttp(broker, config.Object, logger.Object);
            try
            {
                nanoHttp.Start();
                Socket client = new Socket(SocketType.Stream, ProtocolType.IP)
                {
                    ReceiveBufferSize = 1024,
                    LingerState = new LingerOption(false, 0),
                    NoDelay = true
                };
                client.Connect("127.0.0.1", port);

                byte[] bytes = Encoding.ASCII.GetBytes(TestRequestHugeContent);

                client.Send(bytes);

                broker.WaitHandle.WaitOne();

                client.Shutdown(SocketShutdown.Both);
                client.Dispose();
            }
            finally
            {
                nanoHttp.Stop();
            }
            Assert.AreEqual(HttpVerb.Get, broker.Verb);
            Assert.AreEqual(2000, broker.Body.Length);
            Assert.AreEqual(48, broker.Body[0]);
            Assert.AreEqual(57, broker.Body[1999]);
        }

        [TestMethod]
        public void TestProcessingGetRequestWithConsignmentMessageWithSignature()
        {
            string content = File.ReadAllText("BigJsonExample.json");
            string TestConsignmentWithSignatureRequest =
                $"GET /description HTTP/1.1\r\nHost: localhost\r\nContent-Length: {content.Length}\r\n\r\n{content}";

            MockRequestBroker broker = new MockRequestBroker();

            Mock<IConfigurationManager> config = new Mock<IConfigurationManager>();
            int port = 8883;
            config.Setup(conf => conf.AppSettings["Port"]).Returns(port.ToString);
            Mock<ILogger> logger = new Mock<ILogger>();
            NanoHttp nanoHttp = new NanoHttp(broker, config.Object, logger.Object);
            try
            {
                nanoHttp.Start();
                Socket client = new Socket(SocketType.Stream, ProtocolType.IP)
                {
                    ReceiveBufferSize = 1024,
                    LingerState = new LingerOption(false, 0),
                    NoDelay = true
                };
                client.Connect("127.0.0.1", port);

                byte[] bytes = Encoding.ASCII.GetBytes(TestConsignmentWithSignatureRequest);

                client.Send(bytes);

                broker.WaitHandle.WaitOne();

                client.Shutdown(SocketShutdown.Both);
                client.Dispose();
            }
            finally
            {
                nanoHttp.Stop();
            }
            Assert.AreEqual(HttpVerb.Get, broker.Verb);
            Assert.AreEqual(13575, broker.Body.Length);
            Assert.AreEqual(123, broker.Body[0]);
            Assert.AreEqual(34, broker.Body[1999]);
        }
    }

    public class MockRequestBroker : IRequestBroker
    {
        public byte[] Body { get; set; }
        public HttpVerb Verb { get; set; }

        public string Path { get; set; }

        public HttpRequest ReceivedRequest { get; set; }

        public EventWaitHandle WaitHandle;

        public MockRequestBroker()
        {
            WaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
        }

        public HttpResponse HandleRequest(HttpRequest header)
        {
            Body = header.Body;
            Path = header.Path;
            Verb = header.Verb;
            ReceivedRequest = header;

            this.WaitHandle.Set();

            Mock<IDateTimeProvider> dtp = new Mock<IDateTimeProvider>();
            dtp.Setup(p => p.UtcNow).Returns(DateTime.UtcNow);
            return new HttpResponse(dtp.Object);
        }
    }
}
