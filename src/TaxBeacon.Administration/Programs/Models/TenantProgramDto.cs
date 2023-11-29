using Mapster;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Administration;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.Administration.Programs.Models;

public record TenantProgramDto: IRegister
{
    public Guid Id { get; init; }

    public string Name { get; init; } = null!;

    public string? Reference { get; init; }

    public string? Overview { get; init; }

    public string? LegalAuthority { get; init; }

    public string? Agency { get; init; }

    public Jurisdiction Jurisdiction { get; init; }

    public string? JurisdictionName { get; init; }

    public string? IncentivesArea { get; init; }

    public string? IncentivesType { get; init; }

    public Status Status { get; init; }

    public DateTime? StartDateTimeUtc { get; init; }

    public DateTime? EndDateTimeUtc { get; init; }

    public DateTime? DeactivationDateTimeUtc { get; init; }

    public DateTime? ReactivationDateTimeUtc { get; init; }

    public string? Department { get; init; }

    public string? ServiceArea { get; init; }

    public void Register(TypeAdapterConfig config) =>
        config.NewConfig<TenantProgram, TenantProgramDto>()
            .Map(dest => dest.Id, src => src.Program.Id)
            .Map(dest => dest.Name, src => src.Program.Name)
            .Map(dest => dest.Reference, src => src.Program.Reference)
            .Map(dest => dest.Overview, src => src.Program.Overview)
            .Map(dest => dest.LegalAuthority, src => src.Program.LegalAuthority)
            .Map(dest => dest.Agency, src => src.Program.Agency)
            .Map(dest => dest.Jurisdiction, src => src.Program.Jurisdiction)
            .Map(dest => dest.JurisdictionName, src => src.Program.JurisdictionName)
            .Map(dest => dest.IncentivesArea, src => src.Program.IncentivesArea)
            .Map(dest => dest.IncentivesType, src => src.Program.IncentivesType)
            .Map(dest => dest.StartDateTimeUtc, src => src.Program.StartDateTimeUtc)
            .Map(dest => dest.EndDateTimeUtc, src => src.Program.EndDateTimeUtc)
            .Map(dest => dest.Status, src => src.Status)
            .Map(dest => dest.Department,
                src => src.DepartmentTenantPrograms.Where(d => d.IsDeleted != true).Select(d => d.Department.Name)
                    .SingleOrDefault())
            .Map(dest => dest.ServiceArea,
                src => src.ServiceAreaTenantPrograms.Where(d => d.IsDeleted != true).Select(d => d.ServiceArea.Name)
                    .SingleOrDefault());
}
