﻿using System.Text.Json;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Models;
using TaxBeacon.UserManagement.Services.Program.Activities.Models;

namespace TaxBeacon.UserManagement.Services.Program.Activities;

public sealed class ProgramCreatedEventFactory: IProgramActivityFactory
{
    public uint Revision => 1;

    public ProgramEventType EventType => ProgramEventType.ProgramCreatedEvent;

    public ActivityItemDto Create(string programEvent)
    {
        var programCreatedEvent = JsonSerializer.Deserialize<ProgramCreatedEvent>(programEvent);

        return new ActivityItemDto
        (
            Date: programCreatedEvent!.CreatedDate,
            FullName: programCreatedEvent.ExecutorFullName,
            Message: programCreatedEvent.ToString()
        );
    }
}
