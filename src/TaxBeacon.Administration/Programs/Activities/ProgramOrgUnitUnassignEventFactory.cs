﻿using System.Text.Json;
using TaxBeacon.Administration.Programs.Activities.Models;
using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Administration.Programs.Activities;

public class ProgramOrgUnitUnassignEventFactory: IProgramActivityFactory
{
    public uint Revision => 1;

    public ProgramEventType EventType => ProgramEventType.ProgramOrgUnitUnassignEvent;

    public ActivityItemDto Create(string programEvent)
    {
        var tenantProgramUnassignEvent = JsonSerializer.Deserialize<ProgramOrgUnitUnassignEvent>(programEvent);

        return new ActivityItemDto
        (
            Date: tenantProgramUnassignEvent!.UnassignDateTime,
            FullName: tenantProgramUnassignEvent.ExecutorFullName,
            Message: tenantProgramUnassignEvent.ToString()
        );
    }
}
