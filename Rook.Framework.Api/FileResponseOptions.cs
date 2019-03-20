namespace Rook.Framework.Api
{
    public class FileResponseOptions
    {
        public string AttachmentFilename { get; set; }
        public string ContentType { get; set; } = "application/octet-stream";
    }
}
