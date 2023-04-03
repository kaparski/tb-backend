using TaxBeacon.Common.Services;

namespace TaxBeacon.UserManagement.Models.Activities;

public class UnassignUsersEvent
{
    public UnassignUsersEvent(string roles, Guid unassignedByUserId, string fullName, DateTime unassignDate)
    {
        UnassignDate = unassignDate;
        UnassignedRoles = roles;
        UnassignedByUserId = unassignedByUserId;
        UnassignedByFullName = fullName;
    }

    public DateTime UnassignDate { get; set; }

    public string UnassignedByFullName { get; set; }

    public Guid UnassignedByUserId { get; set; }

    public string UnassignedRoles { get; set; }

    public string ToString(IDateTimeFormatter dateTimeFormatter) => $"User was unassigned from the following roles: {UnassignedRoles} by {UnassignedByFullName} "
                                                                    + $"{dateTimeFormatter.FormatDate(UnassignDate)}";
}
