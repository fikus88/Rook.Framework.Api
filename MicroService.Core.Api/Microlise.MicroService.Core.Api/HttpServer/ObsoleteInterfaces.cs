using System;

namespace Microlise.MicroService.Core.Api.HttpServer
{
    [Obsolete("IHttpRequest has been moved to Microlise.MicroService.Core.HttpServer.IHttpRequest", true)]
    public interface IHttpRequest { }

    [Obsolete("IHttpResponse has been moved to Microlise.MicroService.Core.HttpServer.IHttpResponse", true)]
    public interface IHttpResponse { }

    [Obsolete("INanoHttp has been moved to Microlise.MicroService.Core.HttpServer.INanoHttp", true)]
    public interface INanoHttp { }

    [Obsolete("HttpVerb has been moved to Microlise.MicroService.Core.HttpServer.HttpVerb", true)]
    public enum HttpVerb { }
}
