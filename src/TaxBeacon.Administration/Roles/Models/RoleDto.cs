﻿using TaxBeacon.Common.Enums;

namespace TaxBeacon.Administration.Roles.Models;

public class RoleDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public int AssignedUsersCount { get; set; }

    public SourceType Type { get; set; }
}
