using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.UserManagement.Users.Activities.Factories;

public interface IUserActivityFactory
{
    public uint Revision { get; }

    public UserEventType UserEventType { get; }

    public ActivityItemDto Create(string userEvent);
}
