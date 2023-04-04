using TaxBeacon.Common.Services;

namespace TaxBeacon.UserManagement.Models.Activities
{
    public sealed class UserCreatedEvent
    {
        public Guid CreatedById { get; }

        public string CreatedUserEmail { get; }

        public DateTime CreatedDate { get; }

        public string FullName { get; }

        public string Roles { get; }

        public UserCreatedEvent(Guid createdById, string createdUserEmail, DateTime createdDate, string fullName, string roles)
        {
            CreatedById = createdById;
            CreatedUserEmail = createdUserEmail;
            CreatedDate = createdDate;
            FullName = fullName;
            Roles = roles;
        }

        public string ToString(IDateTimeFormatter dateTimeFormatter) => $"{dateTimeFormatter.FormatDate(CreatedDate)} User created by {FullName} {Roles}";
    }
}
