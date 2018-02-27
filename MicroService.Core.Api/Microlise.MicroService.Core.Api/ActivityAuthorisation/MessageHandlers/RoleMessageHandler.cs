using Microlise.MicroService.Core.Application.Message;
using Microlise.MicroService.Core.Application.MessageHandlers;
using Microlise.MicroService.Core.Attributes;

namespace Microlise.MicroService.Core.Api.ActivityAuthorisation.MessageHandlers {
    [Handler("RegisterActivityForAuthorisation", AcceptanceBehaviour = AcceptanceBehaviour.OnlyWithSolution)]
    [Handler("GetRolesForActivity", AcceptanceBehaviour = AcceptanceBehaviour.OnlyWithoutSolution)]
    internal class RoleMessageHandler : IMessageHandler2<string, string> {
        private readonly IActivityAuthorisationManager activityAuthorisationManager;
        public RoleMessageHandler(IActivityAuthorisationManager activityAuthorisationManager)
        {
            this.activityAuthorisationManager = activityAuthorisationManager;
        }

        public CompletionAction Handle(Message<string, string> message)
        {
            activityAuthorisationManager.ActivityRoles[message.Need] = message.Solution;
            return CompletionAction.DoNothing;
        }
    }
}