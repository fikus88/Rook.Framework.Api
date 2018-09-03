using Microlise.MicroService.Core.Application.Message;
using Microlise.MicroService.Core.Application.MessageHandlers;
using Microlise.MicroService.Core.Attributes;
using Microlise.MicroService.Core.Backplane;

namespace Microlise.MicroService.Core.Api.ActivityAuthorisation.MessageHandlers
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