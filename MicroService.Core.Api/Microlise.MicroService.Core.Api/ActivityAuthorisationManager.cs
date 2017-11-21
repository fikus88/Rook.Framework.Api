using System;
using System.Collections.Generic;
using Microlise.MicroService.Core.Common;
using Microlise.MicroService.Core.Application.Bus;
using Microlise.MicroService.Core.Attributes;
using Microlise.MicroService.Core.Application.MessageHandlers;
using Microlise.MicroService.Core.Application.Message;
using Microlise.MicroService.Core.IoC;

namespace Microlise.MicroService.Core.Api
{
    [Handler("RegisterActivityForAuthorisation")]
    internal class ActivityAuthorisationManager : IMessageHandler<string, string>
    {
        private readonly IQueueWrapper queue;
        private readonly IRoleRepository roleRepository;

        public ActivityAuthorisationManager(IQueueWrapper queueWrapper, IRoleRepository roleRepository)
        {
            queue = queueWrapper;
            this.roleRepository = roleRepository;
        }

        public void Initialise()
        {
            // Get all the Activities
            Dictionary<Type, ActivityHandlerAttribute[]> attributes = Container.FindAttributedTypes<ActivityHandlerAttribute>();

            // Send a "Register" message on the bus for each activity
            foreach (KeyValuePair<Type, ActivityHandlerAttribute[]> keyValuePair in attributes)
            foreach (ActivityHandlerAttribute attribute in keyValuePair.Value)
                queue.PublishMessage(new Message<string, string>() {
                    Method = "RegisterActivityForAuthorisation",
                    Need = attribute.ActivityName
                });
        }        

        public void Handle(Message<string, string> message)
        {
            if (message.Solution == null) return;
            roleRepository.Add(message.Need, message.Solution);
            
        }
    }


}