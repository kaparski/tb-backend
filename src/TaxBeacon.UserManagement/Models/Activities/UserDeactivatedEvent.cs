using TaxBeacon.Common.Services;

namespace TaxBeacon.UserManagement.Models.Activities
{
    public sealed class UserDeactivatedEvent
    {
        public Guid DeactivatedById { get; }

        public DateTime DectivatedDate { get; }

        public string FullName { get; }

        public string Roles { get; }

        public UserDeactivatedEvent(Guid deactivatedById, DateTime dectivatedDate, string fullName, string roles)
        {
            DeactivatedById = deactivatedById;
            DectivatedDate = dectivatedDate;
            FullName = fullName;
            Roles = roles;
        }

        public string ToString(IDateTimeFormatter dateTimeFormatter)
            => $"User deactivated {dateTimeFormatter.FormatDate(DectivatedDate)} by {FullName} {Roles}";
    }
}
