﻿namespace TaxBeacon.Common.Services;

public interface ICurrentUserService
{
    public Guid UserId { get; }
}
