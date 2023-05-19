﻿using System.Text.Json;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Models.Activities.Program;

namespace TaxBeacon.UserManagement.Services.Activities.Program;

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