using System;
using Microlise.MicroService.Core.Application.Message;
using Microlise.MicroService.Core.Application.MessageHandlers;
using Microlise.MicroService.Core.Attributes;
using Microlise.MicroService.Core.Backplane;

namespace Microlise.MicroService.Core.Api.ActivityAuthorisation.MessageHandlers
{
    [Handler("RegisterActivityForAuthorisation", AcceptanceBehaviour = AcceptanceBehaviour.OnlyWithSolution)]
    [Handler("GetRolesForActivity", AcceptanceBehaviour = AcceptanceBehaviour.OnlyWithoutSolution)]
    internal class RoleMessageHandler : IMessageHandler2<string, string>, IBackplaneConsumer
    {
        private readonly IActivityAuthorisationManager activityAuthorisationManager;
        private readonly IBackplane backplane;

        public RoleMessageHandler(IActivityAuthorisationManager activityAuthorisationManager, IBackplane backplane)
        {
            this.activityAuthorisationManager = activityAuthorisationManager;
            this.backplane = backplane;
        }

        public Guid ConsumesType => typeof(ActivityRoles).GUID;

        public void Consume(object v)
        {
            ActivityRoles ar = (ActivityRoles)v;
            activityAuthorisationManager.ActivityRoles[ar.Activity] = ar.Roles;
        }

        public CompletionAction Handle(Message<string, string> message)
        {
            backplane.Send(new ActivityRoles { Activity = message.Need, Roles = message.Solution });
            return CompletionAction.DoNothing;
        }
    }

    public class ActivityRoles
    {
        public string Activity;
        public string[] Roles;
    }
}