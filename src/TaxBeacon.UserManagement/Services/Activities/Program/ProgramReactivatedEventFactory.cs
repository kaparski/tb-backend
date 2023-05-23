﻿using System.Text.Json;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Models.Activities.Program;

namespace TaxBeacon.UserManagement.Services.Activities.Program;

public sealed class ProgramReactivatedEventFactory: IProgramActivityFactory
{
    public uint Revision => 1;

    public ProgramEventType EventType => ProgramEventType.ProgramReactivatedEvent;

    public ActivityItemDto Create(string programEvent)
    {
        var programReactivatedEvent = JsonSerializer.Deserialize<ProgramReactivatedEvent>(programEvent);

        return new ActivityItemDto
        (
            Date: programReactivatedEvent!.ReactivatedDate,
            FullName: programReactivatedEvent.ExecutorFullName,
            Message: programReactivatedEvent.ToString()
        );
    }
}
