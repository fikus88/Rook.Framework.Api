using System;
using System.Net;

namespace Microlise.MicroService.Core.Api
{
    public interface IRequestStore
    {
        Func<Guid> CreateUniqueId { get; set; }
        bool FindResponse(Guid requestId, object data);
        HttpStatusCode PublishAndWaitForResponse(string baseAddress, dynamic message, HttpStatusCode successResponseCode, out object responseData);
    }
}
