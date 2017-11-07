using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using Microlise.MicroService.Core.Common;
using Microlise.MicroService.Core.Application.Bus;
using Microlise.MicroService.Core.Attributes;
using Microlise.MicroService.Core.Application.MessageHandlers;
using Microlise.MicroService.Core.Application.Message;

namespace Microlise.MicroService.Core.Api
{
    [Handler("RegisterActivityForAuthorisation")]
    internal class ActivityAuthorisationManager : IMessageHandler<string, string>
    {
        private AutoDictionary<string, IEnumerable<string>> activityRoles = new AutoDictionary<string, IEnumerable<string>>();

        private IQueueWrapper queue;

        public ActivityAuthorisationManager(IQueueWrapper queueWrapper)
        {
            queue = queueWrapper;
        }

        public void Initialise()
        {
            // Get all the Activities

            // Send a "Register" message on the bus
            queue.PublishMessage(new Message<string, string>() { Need = "ActivityNameGoesHere" });
        }

        public bool CheckAuthorisation(JwtSecurityToken token, ActivityHandlerAttribute attribute)
        {
            return true;
        }

        public void Handle(Message<string, string> message)
        {
            if (message.Solution == null) return;
            activityRoles.Add(message.Need, message.Solution);
        }
    }


}