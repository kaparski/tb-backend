using TaxBeacon.Common.Enums;

namespace TaxBeacon.UserManagement.Services.Program.Models;

public record CreateProgramDto(
    string Name,
    string? Reference,
    string? Overview,
    string? LegalAuthority,
    string Agency,
    Jurisdiction Jurisdiction,
    string? State,
    string? County,
    string? City,
    string? IncentivesArea,
    string? IncentivesType,
    DateTime? StartDateTimeUtc,
    DateTime? EndDateTimeUtc);
