﻿using Mapster;
using TaxBeacon.Common.Enums;
using TaxBeacon.DAL.Entities;

namespace TaxBeacon.UserManagement.Models;

public class UserDto
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LegalName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public DateTime CreatedDateTimeUtc { get; set; }

    public string Email { get; set; } = null!;

    public Status Status { get; set; }

    public DateTime? LastLoginDateTimeUtc { get; set; }

    public string FullName { get; init; } = null!;

    public DateTime? DeactivationDateTimeUtc { get; set; }

    public DateTime? ReactivationDateTimeUtc { get; set; }

    public string? Roles { get; set; }

    public string? RoleNamesAsString { get; set; }

    public string? RoleIdsAsString { get; set; }

    public IEnumerable<Guid>? RoleIds { get; set; }

    public IEnumerable<string>? RoleNames { get; set; }

    public Guid? DepartmentId { get; set; }

    public string? Department { get; set; }

    public Guid? DivisionId { get; set; }

    public string? Division { get; set; }

    public Guid? JobTitleId { get; set; }

    public string? JobTitle { get; set; }

    public Guid? ServiceAreaId { get; set; }

    public string? ServiceArea { get; set; }

    public Guid? TeamId { get; set; }

    public string? Team { get; set; }
}
