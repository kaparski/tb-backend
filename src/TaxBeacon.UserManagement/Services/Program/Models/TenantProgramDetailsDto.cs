using Mapster;
using TaxBeacon.Common.Enums;
using TaxBeacon.DAL.Entities;

namespace TaxBeacon.UserManagement.Services.Program.Models;

public class TenantProgramDetailsDto: IRegister
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Reference { get; set; }

    public string? Overview { get; set; }

    public string? LegalAuthority { get; set; }

    public string? Agency { get; set; }

    public string? Jurisdiction { get; set; }

    public string? State { get; set; }

    public string? County { get; set; }

    public string? City { get; set; }

    public string? IncentivesArea { get; set; }

    public string? IncentivesType { get; set; }

    public DateTime StartDateTimeUtc { get; set; }

    public DateTime EndDateTimeUtc { get; set; }

    public DateTime CreatedDateTimeUtc { get; set; }

    public DateTime? DeactivationDateTimeUtc { get; set; }

    public DateTime? ReactivationDateTimeUtc { get; set; }

    public Status Status { get; set; }
    public void Register(TypeAdapterConfig config) =>
        config.NewConfig<TenantProgram, TenantProgramDetailsDto>()
            .Map(dest => dest.Id, src => src.Program.Id)
            .Map(dest => dest.Name, src => src.Program.Name)
            .Map(dest => dest.Reference, src => src.Program.Reference)
            .Map(dest => dest.Overview, src => src.Program.Overview)
            .Map(dest => dest.LegalAuthority, src => src.Program.LegalAuthority)
            .Map(dest => dest.Agency, src => src.Program.Agency)
            .Map(dest => dest.Jurisdiction, src => src.Program.Jurisdiction)
            .Map(dest => dest.State, src => src.Program.State)
            .Map(dest => dest.County, src => src.Program.County)
            .Map(dest => dest.City, src => src.Program.City)
            .Map(dest => dest.IncentivesArea, src => src.Program.IncentivesArea)
            .Map(dest => dest.IncentivesType, src => src.Program.IncentivesType)
            .Map(dest => dest.StartDateTimeUtc, src => src.Program.StartDateTimeUtc)
            .Map(dest => dest.EndDateTimeUtc, src => src.Program.EndDateTimeUtc)
            .Map(dest => dest.CreatedDateTimeUtc, src => src.Program.CreatedDateTimeUtc)
            .Map(dest => dest.DeactivationDateTimeUtc, src => src.DeactivationDateTimeUtc)
            .Map(dest => dest.ReactivationDateTimeUtc, src => src.ReactivationDateTimeUtc)
            .Map(dest => dest.Status, src => src.Status);
}
