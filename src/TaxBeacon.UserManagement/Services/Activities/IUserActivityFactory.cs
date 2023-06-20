﻿using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.UserManagement.Services.Activities;

public interface IUserActivityFactory
{
    public uint Revision { get; }

    public UserEventType UserEventType { get; }

    public ActivityItemDto Create(string userEvent);
}
