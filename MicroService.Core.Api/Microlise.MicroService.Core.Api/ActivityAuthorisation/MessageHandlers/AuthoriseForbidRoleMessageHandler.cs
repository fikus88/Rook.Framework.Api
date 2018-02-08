using Microlise.MicroService.Core.Application.Message;
using Microlise.MicroService.Core.Application.MessageHandlers;
using Microlise.MicroService.Core.Attributes;

namespace Microlise.MicroService.Core.Api.ActivityAuthorisation.MessageHandlers {
    [Handler("ForbidRoleToPerformActivity", AcceptanceBehaviour = AcceptanceBehaviour.OnlyWithSolution)]
    [Handler("AuthoriseRoleToPerformActivity", AcceptanceBehaviour = AcceptanceBehaviour.OnlyWithSolution)]
    internal class AuthoriseForbidRoleMessageHandler : IMessageHandler2<ActivityRoleNeed, string>
    {
        private readonly IActivityAuthorisationManager activityAuthorisationManager;
        public AuthoriseForbidRoleMessageHandler(IActivityAuthorisationManager activityAuthorisationManager)
        {
            this.activityAuthorisationManager = activityAuthorisationManager;
        }

        public CompletionAction Handle(Message<ActivityRoleNeed, string> message)
        {
            activityAuthorisationManager.ActivityRoles[message.Need.ActivityName] = message.Solution;
            return CompletionAction.DoNothing;
        }
    }
}