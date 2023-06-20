using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.UserManagement.Services.Activities
{
    public interface ITeamActivityFactory
    {
        public uint Revision { get; }

        public TeamEventType EventType { get; }

        public ActivityItemDto Create(string userEvent);
    }
}
