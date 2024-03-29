﻿using System;
using System.Net;
using Rook.Framework.Core;
using Rook.Framework.Api;
using Rook.Framework.Core.HttpServer;
using IRequestStore = Rook.Framework.Api.IRequestStore;

namespace Rook.Example.ExampleAPI.ActivityHandlers
{
    [ActivityHandler("StoreWidget", HttpVerb.Post, "/widget/{widgetId}", "Puts a Widget in the Widget Store", "name", "weight")]
    public class PutWidget : PassThroughActivityHandler
    {
        public PutWidget(IRequestStore requestStore) : base(requestStore) { }
        protected override string Method => "StoreWidget";
        protected override HttpStatusCode SuccessStatusCode => HttpStatusCode.OK;
        protected override ResponseStyle ResponseStyle => ResponseStyle.FirstOrDefault;
        public override dynamic ExampleRequestDocument { get; } = new { name = "NewWidget123", owner = Guid.NewGuid(), version = "1.43.2.5", width = 145.2, weight = 10 };
        public override dynamic ExampleResponseDocument { get; } = new { id = Guid.NewGuid(), name = "NewWidget123", owner = Guid.NewGuid(), version = "1.43.2.5", width = 145.2, weight = 10 };
    }
}