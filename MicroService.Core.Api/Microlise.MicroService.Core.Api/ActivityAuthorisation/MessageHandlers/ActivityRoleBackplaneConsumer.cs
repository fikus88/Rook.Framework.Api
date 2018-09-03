using Microlise.MicroService.Core.Backplane;

namespace Microlise.MicroService.Core.Api.ActivityAuthorisation.MessageHandlers
{
    internal class ActivityRoleBackplaneConsumer : BackplaneConsumer<ActivityRoles>
    {
        private readonly IActivityAuthorisationManager _activityAuthorisationManager;

        public ActivityRoleBackplaneConsumer(IActivityAuthorisationManager activityAuthorisationManager)
        {
            _activityAuthorisationManager = activityAuthorisationManager;
        }

        public override void Consume(ActivityRoles value)
        {
            _activityAuthorisationManager.ActivityRoles[value.Activity] = value.Roles;
        }
    }
}