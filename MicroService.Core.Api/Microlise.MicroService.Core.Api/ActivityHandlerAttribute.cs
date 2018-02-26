using System;
using Microlise.MicroService.Core.Api.HttpServer;

namespace Microlise.MicroService.Core.Api
{
    [Obsolete("Use ActivityHandlerAttribute instead", true)]
    public sealed class VerbHandlerAttribute : ActivityHandlerAttribute
    {
        public VerbHandlerAttribute(string activityName, HttpVerb verb, string path, string description = null, params string[] expectedParameters) : base(activityName, verb, path, description, expectedParameters) { }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ActivityHandlerAttribute : Attribute
    {
        /// <summary>
        /// Activity Handler Attribute
        /// </summary>
        /// <param name="activityName">The MUP-wide unique name of the activity</param>
        /// <param name="verb">The verb that will be handled - e.g. GET requests, POST requests, etc</param>
        /// <param name="path">The path that will be handled with URL parameters delimited with curly braces, e.g. /user/<i>{userId}</i></param>
        /// <param name="description">A human-readable description of the behaviour of the handler</param>
        /// <param name="expectedParameters">Parameters expected at the end of the URL, e.g. ?<i>userId</i>=123&<i>organisationId</i>=564363</param>
        public ActivityHandlerAttribute(string activityName, HttpVerb verb, string path, string description = null, params string[] expectedParameters)
        {
            Verb = verb;
            Path = path;
            ExpectedParameters = expectedParameters;
            Description = description ?? $"{verb} handler for {path}";
            ActivityName = activityName;
        }

        public string Description { get; }
        public string ActivityName { get; }
        public HttpVerb Verb { get; }
        public string Path { get; }
        /// <summary>
        /// Parameters expected as part of the URL, after the final item in the path, e.g.
        /// /fish?<b>parameter1=value&parameter2=invaluable</b>
        /// </summary>
        public string[] ExpectedParameters { get; }
        public bool AsSingleton { get; set; }
        public bool SkipAuthorisation { get; set; }
    }
}