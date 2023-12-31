﻿using System.Text.Json;
using TaxBeacon.Administration.Users.Activities.Models;
using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Administration.Users.Activities.Factories;

public sealed class AssignRolesEventFactory: IUserActivityFactory
{
    public uint Revision => 1;

    public UserEventType UserEventType => UserEventType.UserRolesAssign;

    public ActivityItemDto Create(string userEvent)
    {
        var assignRolesEventFactory = JsonSerializer.Deserialize<AssignRolesEvent>(userEvent);

        return new ActivityItemDto
        (
            Date: assignRolesEventFactory!.AssignDate,
            FullName: assignRolesEventFactory.ExecutorFullName,
            Message: assignRolesEventFactory.ToString()
        );
    }
}
