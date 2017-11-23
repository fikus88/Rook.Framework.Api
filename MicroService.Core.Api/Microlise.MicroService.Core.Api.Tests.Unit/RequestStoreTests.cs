using System;
using System.Collections.Generic;
using System.Net;
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
        [TestMethod]
        public void PusblishAndRespond()
        {
            Guid uuid = Guid.NewGuid();
            Message<string, string> needMessage = new Message<string, string>() { Need = "Hello", Method = "X", Uuid = uuid };
            
            var dtp = new Mock<IDateTimeProvider>();
            var qw = new Mock<IQueueWrapper>();
            var log = new Mock<ILogger>();
            IMongoStore mongo = Container.GetInstance<IMongoStore>();
            RequestStore rs = new RequestStore(dtp.Object, qw.Object, log.Object, mongo);
            rs.Start();
            IHttpResponse response = new HttpResponse(dtp.Object);
            new TaskFactory().StartNew(() =>
            {
                Thread.Sleep(500);
                mongo.Put(new MessageWrapper { Uuid = needMessage.Uuid, SolutionJson = JsonConvert.SerializeObject(new[] { "World" }) });
            });
            rs.PublishAndWaitForResponse(needMessage, HttpStatusCode.OK, response);
            Assert.AreEqual(HttpStatusCode.OK, response.HttpStatusCode);
        }

    }
}