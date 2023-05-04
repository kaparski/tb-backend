using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.UserManagement.Models;

namespace TaxBeacon.UserManagement.Services.Activities
{
    public interface ITeamActivityFactory
    {
        public uint Revision { get; }

        public TeamEventType EventType { get; }

        public ActivityItemDto Create(string userEvent);
    }
}
