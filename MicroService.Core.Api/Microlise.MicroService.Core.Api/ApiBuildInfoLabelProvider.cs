using Microlise.MicroService.Core.Monitoring;
using System.Collections.Generic;

namespace Microlise.MicroService.Core.Api
{
    /// <summary>
    /// Provides the build info labels for Microlise.MicroService.Core.Api
    /// </summary>
    public class ApiBuildInfoLabelProvider : IBuildInfoLabelProvider
    {
        public IEnumerable<BuildInfoLabel> GetBuildInfoLabels()
        {
            return new[]
            {
                new BuildInfoLabel("core_api_version", GetCoreApiVersion())
            };
        }
        private static string GetCoreApiVersion()
        {
            return typeof(ApiBuildInfoLabelProvider).Assembly.GetName().Version.ToString();
        }
    }
}