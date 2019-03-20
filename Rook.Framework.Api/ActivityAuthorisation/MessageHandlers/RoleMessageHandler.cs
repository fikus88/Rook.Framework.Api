using Rook.Framework.Core.Application.Message;
using Rook.Framework.Core.Application.MessageHandlers;
using Rook.Framework.Core.Attributes;
using Rook.Framework.Core.Backplane;

namespace Rook.Framework.Api.ActivityAuthorisation.MessageHandlers
{
    [Handler("RegisterActivityForAuthorisation", AcceptanceBehaviour = AcceptanceBehaviour.OnlyWithSolution)]
    [Handler("GetRolesForActivity", AcceptanceBehaviour = AcceptanceBehaviour.OnlyWithoutSolution)]
    internal class RoleMessageHandler : IMessageHandler2<string, string>
    {
        private readonly IBackplane backplane;

        public RoleMessageHandler(IBackplane backplane)
        {
            this.backplane = backplane;
        }
        

        public CompletionAction Handle(Message<string, string> message)
        {
            backplane.Send(new ActivityRoles { Activity = message.Need, Roles = message.Solution });
            return CompletionAction.DoNothing;
        }
    }
}