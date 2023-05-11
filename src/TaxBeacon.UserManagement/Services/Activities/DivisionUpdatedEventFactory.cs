﻿using System.Text.Json;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Models.Activities;
using TaxBeacon.UserManagement.Models.Activities.Tenant;

namespace TaxBeacon.UserManagement.Services.Activities
{
    public class DivisionUpdatedEventFactory: IDivisionActivityFactory
    {
        public uint Revision => 1;

        public DivisionEventType EventType => DivisionEventType.DivisionUpdatedEvent;

        public ActivityItemDto Create(string divisionEvent)
        {
            var divisionUpdatedEvent = JsonSerializer.Deserialize<DivisionUpdatedEvent>(divisionEvent);

            return new ActivityItemDto
            (
                Date: divisionUpdatedEvent!.UpdatedDate,
                FullName: divisionUpdatedEvent.ExecutorFullName,
                Message: divisionUpdatedEvent.ToString()
            );
        }
    }
}
