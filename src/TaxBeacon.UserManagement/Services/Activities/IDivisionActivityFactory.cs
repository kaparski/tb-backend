﻿using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.UserManagement.Models;

namespace TaxBeacon.UserManagement.Services.Activities;

public interface IDivisionActivityFactory
{
    public uint Revision { get; }

    public DivisionEventType EventType { get; }

    public ActivityItemDto Create(string userEvent);
}