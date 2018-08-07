using Microlise.MicroService.Core.Api;
using Microlise.MicroService.Core.HttpServer;
using System.IO;

namespace Microlise.Example.ExampleAPI.ActivityHandlers
{
    /// <summary>
    /// Example handler returning a file.  Go to http://localhost:81/file/foo.txt to download
    /// the descriptionExample.json file returned as "foo.txt" (change /foo.txt to alter the attachment name in the download).
    /// Demonstrates how to use the FileHttpContent implementation of IHttpContent to return files without having to
    /// load the file bytes into memory
    /// </summary>
    [ActivityHandler("GetFile", HttpVerb.Get, "/file/{fileId}")]
    public class GetFile : MicroService.Core.HttpServer.IActivityHandler
    {
        public void Handle(IHttpRequest request, IHttpResponse response)
        {
            var fileMetadata = GetFileMetadata(request.Parameters["fileId"]);

            // FileHttpContent expects the file to exist, so check we can get it
            // and return a 404 if not
            if (File.Exists(fileMetadata.Filename))
            {
                // So send the file to the client
                response.SetFileResponse(fileMetadata.Filename, new FileResponseOptions
                {
                    // Give it a different filename on the download if needed:
                    AttachmentFilename = fileMetadata.AttachmentFilename,

                    // Set a content type if needed
                    ContentType = "application/octet-stream"
                });
            }
            else
            {
                response.SetNotFoundResponse();
            }
        }

        private FakeFileMetadata GetFileMetadata(string fileId)
        {
            // Example with hardcoded value. In a real service this would likely
            // be taken from a solution returned from a need placed on the bus
            return new FakeFileMetadata
            {   
                Filename = "descriptionExample.json",
                AttachmentFilename = $"{fileId}.txt"
            };
        }

        /// <summary>
        /// Example file meta data model containing full filename, and the
        /// name to use in the Content-Disposition name
        /// </summary>
        private class FakeFileMetadata
        {
            public string Filename { get; set; }
            public string AttachmentFilename { get; set; }
        }

        public dynamic ExampleRequestDocument => null;

        public dynamic ExampleResponseDocument => null;
    }
}
