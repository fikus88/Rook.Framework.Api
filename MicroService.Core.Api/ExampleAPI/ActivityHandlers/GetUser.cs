﻿using System;
using Microlise.MicroService.Core.Api;
using Microlise.MicroService.Core.HttpServer;

namespace Microlise.Example.ExampleAPI.ActivityHandlers
{
    [ActivityHandler("GetUser", HttpVerb.Get, "/user/{userId}")]
    public class GetUser : MicroService.Core.HttpServer.IActivityHandler
    {
        public void Handle(IHttpRequest request, IHttpResponse response)
        {
            response.SetObjectContent(new { userId = int.Parse(request.Parameters["userId"]), userName = request.Parameters["name"] });
        }

        public dynamic ExampleRequestDocument { get; } = null;

        public dynamic ExampleResponseDocument { get; } = new
        {
            userId = Guid.NewGuid(),
            userPassword = "482c811da5d5b4bc6d497ffa98491e38",
            userLogin = "dave@tmc",
            lastLoggedIn = DateTime.UtcNow.AddDays(-10).ToString("s")
        };
    }
}
