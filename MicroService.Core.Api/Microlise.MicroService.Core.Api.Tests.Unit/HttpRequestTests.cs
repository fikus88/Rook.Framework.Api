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
            int port = 8881;
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
            int port = 8881;
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

            this.WaitHandle.Set();

            Mock<IDateTimeProvider> dtp = new Mock<IDateTimeProvider>();
            dtp.Setup(p => p.UtcNow).Returns(DateTime.UtcNow);
            return new HttpResponse(dtp.Object);
        }
    }
}
