using TaxBeacon.Common.Services;

namespace TaxBeacon.UserManagement.Models.Activities
{
    public sealed class UserDeactivatedEvent
    {
        public Guid DeactivatedById { get; }

        public DateTime ReactivatedDate { get; }

        public string FullName { get; }

        public string Roles { get; }

        public UserDeactivatedEvent(Guid deactivatedById, DateTime reactivatedDate, string fullName, string roles)
        {
            DeactivatedById = deactivatedById;
            ReactivatedDate = reactivatedDate;
            FullName = fullName;
            Roles = roles;
        }

        public string ToString(IDateTimeFormatter dateTimeFormatter)
            => $"User deactivated {dateTimeFormatter.FormatDate(ReactivatedDate)} by {FullName} {Roles}";
    }
}
