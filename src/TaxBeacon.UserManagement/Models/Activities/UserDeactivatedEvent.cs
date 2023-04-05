namespace TaxBeacon.UserManagement.Models.Activities
{
    public sealed class UserDeactivatedEvent: UserEventBase
    {
        public DateTime DeactivatedDate { get; }

        public UserDeactivatedEvent(Guid executorId, DateTime deactivatedDate, string executorFullName, string executorRoles)
            : base(executorId, executorFullName, executorRoles) => DeactivatedDate = deactivatedDate;

        public override string ToString() => $"User deactivated";
    }
}
