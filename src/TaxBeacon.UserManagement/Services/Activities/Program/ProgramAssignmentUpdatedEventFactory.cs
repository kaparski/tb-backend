﻿using System.Text.Json;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Models.Activities.Program;

namespace TaxBeacon.UserManagement.Services.Activities.Program;

public sealed class ProgramAssignmentUpdatedEventFactory: IProgramActivityFactory
{
    public uint Revision => 1;

    public ProgramEventType EventType => ProgramEventType.ProgramAssignmentUpdatedEvent;

    public ActivityItemDto Create(string programEvent)
    {
        var assignmentUpdatedEvent = JsonSerializer.Deserialize<ProgramAssignmentUpdatedEvent>(programEvent);

        return new ActivityItemDto
        (
            Date: assignmentUpdatedEvent!.UpdatedDate,
            FullName: assignmentUpdatedEvent.ExecutorFullName,
            Message: assignmentUpdatedEvent.ToString()
        );
    }
}
