﻿using Microlise.MicroService.Core.Api.HttpServer;
using System;

namespace Microlise.MicroService.Core.Api
{
    [Obsolete("Use IActivityHandler", true)]
    public interface IVerbHandler : IActivityHandler { }

    public interface IActivityHandler
    {
        void Handle(IHttpRequest request, IHttpResponse response);

        //List<Tuple<string, string>> CreateExampleInputsAndOutputs();
    }
}