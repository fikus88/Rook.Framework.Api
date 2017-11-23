using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microlise.MicroService.Core.Api.HttpServer;
using Microlise.MicroService.Core.Application.Bus;
using Microlise.MicroService.Core.Application.Message;
using Microlise.MicroService.Core.Common;
using Microlise.MicroService.Core.Data;
using Microlise.MicroService.Core.IoC;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

namespace Microlise.MicroService.Core.Api.Tests.Unit
{
    [TestClass]
    public class RequestStoreTests
    {
        private readonly Mock<IDateTimeProvider> dtp = new Mock<IDateTimeProvider>();
        private readonly Mock<IQueueWrapper> qw = new Mock<IQueueWrapper>();
        private readonly Mock<ILogger> log = new Mock<ILogger>();
        private readonly IMongoStore mongo = Container.GetInstance<IMongoStore>();
        private RequestStore rs;

        [TestInitialize]
        public void init()
        {
            rs = new RequestStore(dtp.Object, qw.Object, log.Object, mongo,Container.GetInstance<IRequestMatcher>());
            rs.Start();
        }

        [TestMethod]
        public void PublishAndRespond()
        {
            Guid uuid = Guid.NewGuid();
            Message<string, string> needMessage = new Message<string, string> { Need = "Hello", Method = "X", Uuid = uuid };

            IHttpResponse response = new HttpResponse(dtp.Object);
            new TaskFactory().StartNew(() =>
            {
                Thread.Sleep(500);
                mongo.Put(new MessageWrapper { Uuid = needMessage.Uuid, SolutionJson = JsonConvert.SerializeObject(new[] { "World" }) });
            });
            rs.PublishAndWaitForResponse(needMessage, HttpStatusCode.OK, response);
            Assert.AreEqual(HttpStatusCode.OK, response.HttpStatusCode);
        }

        [TestMethod]
        public void PublishAndRespondWhenResponseIsVeryFast()
        {
            Guid uuid = Guid.NewGuid();
            Message<string, string> needMessage = new Message<string, string> { Need = "Hello", Method = "Y", Uuid = uuid };
            
            IHttpResponse response = new HttpResponse(dtp.Object);
            mongo.Put(new MessageWrapper { Uuid = needMessage.Uuid, SolutionJson = JsonConvert.SerializeObject(new[] { "World" }) });
            rs.PublishAndWaitForResponse(needMessage, HttpStatusCode.OK, response);
            Assert.AreEqual(HttpStatusCode.OK, response.HttpStatusCode);
        }

        [TestMethod]
        public void PublishAndRespondWithComplexNeedAndObjectSolution()
        {
            Guid uuid = Guid.NewGuid();
            Message<NeedObj, object> needMessage = new Message<NeedObj, object> { Need = new NeedObj(), Method = "Z", Uuid = uuid };

            IHttpResponse response = new HttpResponse(dtp.Object);
            new TaskFactory().StartNew(() =>
            {
                Thread.Sleep(500);
                mongo.Put(new MessageWrapper { Uuid = needMessage.Uuid, SolutionJson = JsonConvert.SerializeObject(new { answer = "Hello World" }) });
            });
            rs.PublishAndWaitForResponse(needMessage, HttpStatusCode.OK, response);
            Assert.AreEqual(HttpStatusCode.OK, response.HttpStatusCode);
            Assert.AreEqual("{\"answer\":\"Hello World\"}", Encoding.ASCII.GetString(response.Content));
        }

        public class NeedObj
        {
            public Guid Guid = Guid.NewGuid();
        }
    }
}