using System;
using Microlise.MicroService.Core.Api.HttpServer;

namespace Microlise.MicroService.Core.Api
{
    [Obsolete("Use ActivityHandlerAttribute instead", true)]
    public sealed class VerbHandlerAttribute : ActivityHandlerAttribute
    {
        public VerbHandlerAttribute(string ActivityName, HttpVerb verb, string path, string description = null) : base(ActivityName, verb, path, description) { }
    }
    public class ActivityHandlerAttribute : Attribute
    {
        public ActivityHandlerAttribute(string ActivityName, HttpVerb verb, string path, string description = null)
        {
            Verb = verb;
            Path = path;
            Description = description ?? $"{verb} handler for {path}";
        }

        public string Description { get; }
        public string ActivityName { get; }
        public HttpVerb Verb { get; }
        public string Path { get; }
        public bool AsSingleton { get; set; }
    }
}