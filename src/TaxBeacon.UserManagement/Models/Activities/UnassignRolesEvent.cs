using TaxBeacon.Common.Services;

namespace TaxBeacon.UserManagement.Models.Activities;

public class UnassignUsersEvent
{
    public UnassignUsersEvent(string unassignedRoles, Guid unassignedByUserId, string unassignedByFullName, DateTime unassignDate)
    {
        UnassignDate = unassignDate;
        UnassignedUnassignedRoles = unassignedRoles;
        UnassignedByUserId = unassignedByUserId;
        UnassignedByUnassignedByFullName = unassignedByFullName;
    }

    public DateTime UnassignDate { get; set; }

    public string UnassignedByUnassignedByFullName { get; set; }

    public Guid UnassignedByUserId { get; set; }

    public string UnassignedUnassignedRoles { get; set; }

    public string ToString(IDateTimeFormatter dateTimeFormatter) => $"User was unassigned from the following roles: {UnassignedUnassignedRoles} by {UnassignedByUnassignedByFullName} "
                                                                    + $"{dateTimeFormatter.FormatDate(UnassignDate)}";
}
