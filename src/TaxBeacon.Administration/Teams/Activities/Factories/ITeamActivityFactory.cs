using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Administration.Teams.Activities.Factories;

public interface ITeamActivityFactory
{
    public uint Revision { get; }

    public TeamEventType EventType { get; }

    public ActivityItemDto Create(string userEvent);
}