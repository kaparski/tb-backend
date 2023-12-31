﻿namespace TaxBeacon.Administration.ServiceAreas.Models;

public class ServiceAreaUserDto
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Team { get; set; } = string.Empty;

    public string JobTitle { get; set; } = string.Empty;
}
