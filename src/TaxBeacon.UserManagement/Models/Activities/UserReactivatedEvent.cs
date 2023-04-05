namespace TaxBeacon.UserManagement.Models.Activities
{
    public sealed class UserReactivatedEvent
    {
        public Guid ReactivatedById { get; }

        public DateTime ReactivatedDate { get; }

        public string FullName { get; }

        public string Roles { get; }

        public UserReactivatedEvent(Guid reactivatedById, DateTime reactivatedDate, string fullName, string roles)
        {
            ReactivatedById = reactivatedById;
            ReactivatedDate = reactivatedDate;
            FullName = fullName;
            Roles = roles;
        }

        public override string ToString() => "User reactivated";
    }
}
