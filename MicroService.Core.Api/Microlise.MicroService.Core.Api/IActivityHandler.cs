using System;

namespace Microlise.MicroService.Core.Api
{
    [Obsolete("Use Microlise.MicroService.Core.HttpServer.IActivityHandler", true)]
    public interface IVerbHandler : IActivityHandler { }

    [Obsolete("IActivityHandler has been moved to Microlise.MicroService.Core.HttpServer.IActivityHandler", true)]
    public interface IActivityHandler { }
}
