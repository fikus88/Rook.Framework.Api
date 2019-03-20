using Rook.Framework.Core.Application.Message;
using Rook.Framework.Core.Application.MessageHandlers;
using Rook.Framework.Core.Attributes;

namespace Rook.Framework.Api.ActivityAuthorisation.MessageHandlers
{
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