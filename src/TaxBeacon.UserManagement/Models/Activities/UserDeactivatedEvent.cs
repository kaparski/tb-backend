namespace TaxBeacon.UserManagement.Models.Activities
{
    public sealed class UserDeactivatedEvent
    {
        public Guid DeactivatedById { get; }

        public DateTime DeactivatedDate { get; }

        public string FullName { get; }

        public string Roles { get; }

        public UserDeactivatedEvent(Guid deactivatedById, DateTime deactivatedDate, string fullName, string roles)
        {
            DeactivatedById = deactivatedById;
            DeactivatedDate = deactivatedDate;
            FullName = fullName;
            Roles = roles;
        }

        public override string ToString() => $"User deactivated";
    }
}
