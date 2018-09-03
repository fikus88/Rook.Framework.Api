using Microlise.MicroService.Core.Application.Message;
using Microlise.MicroService.Core.Application.MessageHandlers;
using Microlise.MicroService.Core.Attributes;
using Microlise.MicroService.Core.Backplane;

namespace Microlise.MicroService.Core.Api.ActivityAuthorisation.MessageHandlers
{
    [Handler("RegisterActivityForAuthorisation", AcceptanceBehaviour = AcceptanceBehaviour.OnlyWithSolution)]
    [Handler("GetRolesForActivity", AcceptanceBehaviour = AcceptanceBehaviour.OnlyWithoutSolution)]
    internal class RoleMessageHandler : BackplaneConsumer<ActivityRoles>, IMessageHandler2<string, string>
    {
        private readonly IActivityAuthorisationManager activityAuthorisationManager;
        private readonly IBackplane backplane;

        public RoleMessageHandler(IActivityAuthorisationManager activityAuthorisationManager, IBackplane backplane)
        {
            this.activityAuthorisationManager = activityAuthorisationManager;
            this.backplane = backplane;
        }
        
        public override void Consume(ActivityRoles value)
        {
            activityAuthorisationManager.ActivityRoles[value.Activity] = value.Roles;
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