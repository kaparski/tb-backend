using TaxBeacon.Common.Enums;

namespace TaxBeacon.UserManagement.Programs.Models;

public record UpdateProgramDto(
    string Name,
    string? Reference,
    string? Overview,
    string? LegalAuthority,
    string? Agency,
    Jurisdiction Jurisdiction,
    string? State,
    string? County,
    string? City,
    string? IncentivesArea,
    string? IncentivesType,
    DateTime? StartDateTimeUtc,
    DateTime? EndDateTimeUtc);
