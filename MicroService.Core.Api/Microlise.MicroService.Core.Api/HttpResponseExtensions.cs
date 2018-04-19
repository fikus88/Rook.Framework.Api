using Microlise.MicroService.Core.HttpServer;
using System.Net;

namespace Microlise.MicroService.Core.Api
{
    public static class HttpResponseExtensions
    {
        public static void SetFileResponse(this IHttpResponse response, string filename, FileResponseOptions options = null)
        {
            var fileResponseOptions = options ?? new FileResponseOptions();

            response.HttpStatusCode = HttpStatusCode.OK;

            if (!string.IsNullOrEmpty(options.AttachmentFilename))
            {
                response.Headers.Add("Content-Disposition", $"attachment; filename={options.AttachmentFilename}");
            }

            // FileHttpContent uses a FileStream for transfering the file data to the response,
            // so that the entire file isn't loaded into memory before it gets sent
            // This HttpContent adds Content-Type and Content-Length, so no need to add them manually
            response.HttpContent = new FileHttpContent(filename, options.ContentType);
        }

        public static void SetNotFoundResponse(this IHttpResponse response)
        {
            response.HttpStatusCode = HttpStatusCode.NotFound;
            response.HttpContent = new EmptyHttpContent();
        }        
    }
}
