using Rook.Framework.Core.Monitoring;
using System.Collections.Generic;

namespace Rook.Framework.Api
{
    /// <summary>
    /// Provides the build info labels for Rook.Framework.Api
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