using System.Text;
using Newtonsoft.Json;
using Microlise.MicroService.Core.Common;
using System.Net;
using MicroService.Core.Api.HttpServer;
using System;

namespace Microlise.MicroService.Core.Api
{
    public class ResponseHandler : IRequestHandler
    {
        private readonly IRequestStore _requestStore;
        private readonly ILogger _logger;

        public ResponseHandler(IRequestStore requestStore, ILogger logger)
        {
            _requestStore = requestStore;
            _logger = logger;
        }

        public HttpStatusCode Handle(HttpRequest request, out object responseData)
        {
			byte[] buffer = request.Body;	        
			var message = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(buffer));

			Guid uuid = Guid.Parse(request.Parameters["uuid"]);

			if (message != null) //The ResponseCollector should never send a response without a message
            {
                if (_requestStore.FindResponse(uuid, message))
                {
                    _logger.Trace($"Operation=\"{nameof(ResponseHandler)}.{nameof(Handle)}\" Event=\"Response handled successfully\" MessageId=\"{uuid}\"");
                    //The following response code/data will be returned back to the ResponseCollector
                    responseData = null;
                    return HttpStatusCode.OK;
                }

                _logger.Trace($"Operation=\"{nameof(ResponseHandler)}.{nameof(Handle)}\" Event=\"Response Id not found\" MessageId=\"{uuid}\"");
                //The following response code/data will be returned back to the ResponseCollector
                responseData = "Response Id not found";
                return HttpStatusCode.BadRequest;
            }

            _logger.Trace($"Operation=\"{nameof(ResponseHandler)}.{nameof(Handle)}\" Event=\"Failed to handle response as solution is null\" MessageId=\"{uuid}\"");
            //The following response code/data will be returned back to the ResponseCollector
            responseData = "Response not handled";
            return HttpStatusCode.InternalServerError;
        }
    }
}
