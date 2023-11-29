﻿using System.Text.Json;
using TaxBeacon.Accounts.Entities.Activities.Models;
using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Entities.Activities.Factories;

public sealed class EntityCreatedEventFactory: IEntityActivityFactory
{
    public uint Revision => 1;

    public EntityEventType EventType => EntityEventType.EntityCreated;

    public ActivityItemDto Create(string entityEvent)
    {
        var entityCreatedEvent = JsonSerializer.Deserialize<EntityCreatedEvent>(entityEvent);

        return new ActivityItemDto
        (
            Date: entityCreatedEvent!.CreatedDate,
            FullName: entityCreatedEvent.ExecutorFullName,
            Message: entityCreatedEvent.ToString()
        );
    }
}