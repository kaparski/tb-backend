﻿using System.Text.Json;
using TaxBeacon.Accounts.Contacts.Activities.Models;
using TaxBeacon.Common.Enums.Accounts.Activities;
using TaxBeacon.Common.Models;
using TaxBeacon.Common.Services;

namespace TaxBeacon.Accounts.Contacts.Activities.Factories;

public sealed class ContactUpdatedEventFactory: IActivityFactory<ContactEventType>
{
    public uint Revision => 1;

    public ContactEventType EventType => ContactEventType.ContactUpdated;

    public ActivityItemDto Create(string eventData)
    {
        var contactUpdatedEvent = JsonSerializer.Deserialize<ContactUpdatedEvent>(eventData);

        return new ActivityItemDto
        (
            Date: contactUpdatedEvent!.UpdatedDate,
            FullName: contactUpdatedEvent.ExecutorFullName,
            Message: contactUpdatedEvent.ToString()
        );
    }
}