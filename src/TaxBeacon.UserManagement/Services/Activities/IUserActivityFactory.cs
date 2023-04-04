using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.UserManagement.Models;

namespace TaxBeacon.UserManagement.Services.Activities
{
    public interface IUserActivityFactory
    {
        public uint Revision { get; }

        public EventType EventType { get; }

        public UserActivityDto Create(string userEvent);
    }
}
