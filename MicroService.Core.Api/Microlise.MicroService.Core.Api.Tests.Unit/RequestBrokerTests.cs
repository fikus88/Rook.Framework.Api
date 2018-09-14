using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Reflection;
using Microlise.MicroService.Core.Api.ActivityAuthorisation;
using Microlise.MicroService.Core.Common;
using Microlise.MicroService.Core.HttpServer;
using Microlise.MicroService.Core.StructureMap;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microlise.MicroService.Core.Api.Tests.Unit
{
    [TestClass]
    public class RequestBrokerTests
    {
        private Mock<IActivityAuthorisationManager> _activityAuthorisationManager;
        private Mock<IContainerFacade> _containerFacade;
        private Mock<IConfigurationManager> _configurationManager;
        private Mock<IHttpRequest> _httpRequest;
        private Mock<IHttpResponse> _httpResponse;

        [TestInitialize]
        public void BeforeEachTest()
        {
            var mockRepo = new MockRepository(MockBehavior.Default);
            _activityAuthorisationManager = mockRepo.Create<IActivityAuthorisationManager>();
            _containerFacade = mockRepo.Create<IContainerFacade>();
            _configurationManager = mockRepo.Create<IConfigurationManager>();
            _httpRequest = mockRepo.Create<IHttpRequest>();
            _httpResponse = mockRepo.Create<IHttpResponse>();
            
            _activityAuthorisationManager.Setup(x => x.CheckAuthorisation(It.IsAny<JwtSecurityToken>(), It.IsAny<ActivityHandlerAttribute>())).Returns(true);
            _containerFacade.Setup(x => x.GetAttributedTypes<ActivityHandlerAttribute>(typeof(Core.HttpServer.IActivityHandler))).Returns(
                new Dictionary<Type, ActivityHandlerAttribute[]>
                {
                    {typeof(TestActivityHandler), typeof(TestActivityHandler).GetTypeInfo().GetCustomAttributes().Cast<ActivityHandlerAttribute>().ToArray()}
                });
            _containerFacade.Setup(x => x.GetInstance(typeof(TestActivityHandler))).Returns(new TestActivityHandler());
            _containerFacade.Setup(x => x.GetInstance<IHttpResponse>(true)).Returns(_httpResponse.Object);
        }

        [TestMethod]
        public void HandleRequest_WithBaseUrl_FindsHandlerWithoutBaseUrlHardcoded()
        {
            _httpRequest.Setup(x => x.Path).Returns("/baseurl/test/1");
            _configurationManager.Setup(x => x.Get("BaseUrl", "/")).Returns("/BaseUrl/");

            HttpStatusCode httpStatusOfRequest = HttpStatusCode.BadRequest;
            _httpResponse.SetupSet(response => response.HttpStatusCode = It.IsAny<HttpStatusCode>()).Callback<HttpStatusCode>(value => httpStatusOfRequest = value);

            var sut = new RequestBroker(Mock.Of<ILogger>(), _activityAuthorisationManager.Object, Mock.Of<IApiMetrics>(), _containerFacade.Object, _configurationManager.Object);
            
            sut.HandleRequest(_httpRequest.Object, TokenState.Ok);

            _httpResponse.Verify(x => x.SetStringContent("TEST ACTIVITY BEEN HIT!"), Times.Once);
            Assert.AreEqual(httpStatusOfRequest, HttpStatusCode.OK);
        }

        [TestMethod]
        public void HandleRequest_WithTwoPartBaseUrl_FindsHandlerWithoutBaseUrlHardcoded()
        {
            _httpRequest.Setup(x => x.Path).Returns("/twopart/baseurl/test/1");
            _configurationManager.Setup(x => x.Get("BaseUrl", "/")).Returns("/TwoPart/BaseUrl/");

            HttpStatusCode httpStatusOfRequest = HttpStatusCode.BadRequest;
            _httpResponse.SetupSet(response => response.HttpStatusCode = It.IsAny<HttpStatusCode>()).Callback<HttpStatusCode>(value => httpStatusOfRequest = value);

            var sut = new RequestBroker(Mock.Of<ILogger>(), _activityAuthorisationManager.Object, Mock.Of<IApiMetrics>(), _containerFacade.Object, _configurationManager.Object);
            
            sut.HandleRequest(_httpRequest.Object, TokenState.Ok);

            _httpResponse.Verify(x => x.SetStringContent("TEST ACTIVITY BEEN HIT!"), Times.Once);
            Assert.AreEqual(httpStatusOfRequest, HttpStatusCode.OK);
        }

        [TestMethod]
        public void HandleRequest_WithBaseUrlWithoutSlashes_FindsHandlerWithoutBaseUrlHardcoded()
        {
            _httpRequest.Setup(x => x.Path).Returns("/baseurl/test/1");
            _configurationManager.Setup(x => x.Get("BaseUrl", "/")).Returns("BaseUrl");

            HttpStatusCode httpStatusOfRequest = HttpStatusCode.BadRequest;
            _httpResponse.SetupSet(response => response.HttpStatusCode = It.IsAny<HttpStatusCode>()).Callback<HttpStatusCode>(value => httpStatusOfRequest = value);

            var sut = new RequestBroker(Mock.Of<ILogger>(), _activityAuthorisationManager.Object, Mock.Of<IApiMetrics>(), _containerFacade.Object, _configurationManager.Object);
            
            sut.HandleRequest(_httpRequest.Object, TokenState.Ok);

            _httpResponse.Verify(x => x.SetStringContent("TEST ACTIVITY BEEN HIT!"), Times.Once);
            Assert.AreEqual(httpStatusOfRequest, HttpStatusCode.OK);
        }

        [TestMethod]
        public void HandleRequest_Works_WithDefaultConfig()
        {
            _httpRequest.Setup(x => x.Path).Returns("/test/1");
            _configurationManager.Setup(x => x.Get("BaseUrl", "/")).Returns("/");

            HttpStatusCode httpStatusOfRequest = HttpStatusCode.BadRequest;
            _httpResponse.SetupSet(response => response.HttpStatusCode = It.IsAny<HttpStatusCode>()).Callback<HttpStatusCode>(value => httpStatusOfRequest = value);

            var sut = new RequestBroker(Mock.Of<ILogger>(), _activityAuthorisationManager.Object, Mock.Of<IApiMetrics>(), _containerFacade.Object, _configurationManager.Object);
            
            sut.HandleRequest(_httpRequest.Object, TokenState.Ok);

            _httpResponse.Verify(x => x.SetStringContent("TEST ACTIVITY BEEN HIT!"), Times.Once);
            Assert.AreEqual(httpStatusOfRequest, HttpStatusCode.OK);
        }
    }

    [ActivityHandler("Test", HttpVerb.Get, "/test/{id}")]
    public class TestActivityHandler : Core.HttpServer.IActivityHandler
    {
        public void Handle(IHttpRequest request, IHttpResponse response)
        {
            response.SetStringContent("TEST ACTIVITY BEEN HIT!");
            response.HttpStatusCode = HttpStatusCode.OK;
        }

        public dynamic ExampleRequestDocument { get; } = null;
        public dynamic ExampleResponseDocument { get; } = null;
    }
}
