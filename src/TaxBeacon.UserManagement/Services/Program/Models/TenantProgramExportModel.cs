using Mapster;
using Npoi.Mapper.Attributes;
using TaxBeacon.Common.Enums;
using TaxBeacon.DAL.Entities;

namespace TaxBeacon.UserManagement.Services.Program.Models;

public class TenantProgramExportModel: IRegister
{
    public string Reference { get; set; } = string.Empty;

    [Column("Program Name")]
    public string Name { get; set; } = string.Empty;

    [Column("Program Overview")]
    public string Overview { get; set; } = string.Empty;

    [Column("Legal Authority")]
    public string LegalAuthority { get; set; } = string.Empty;

    [Column("Program Agency")]
    public string Agency { get; set; } = string.Empty;

    public string Jurisdiction { get; set; } = string.Empty;

    [Column("Jurisdiction Name")]
    public string JurisdictionName { get; set; } = string.Empty;

    [Column("Incentives Area")]
    public string IncentivesArea { get; set; } = string.Empty;

    [Column("Incentives Type")]
    public string IncentivesType { get; set; } = string.Empty;

    [Column("Department")]
    public string Department { get; set; } = string.Empty;

    [Column("Service Area")]
    public string ServiceArea { get; set; } = string.Empty;

    [Ignore]
    public DateTime? StartDateTimeUtc { get; set; }

    [Column("Program Start Date")]
    public string StartDateView { get; set; } = string.Empty;

    [Ignore]
    public DateTime? EndDateTimeUtc { get; set; }

    [Column("Program End Date")]
    public string EndDateView { get; set; } = string.Empty;

    [Ignore]
    public DateTime CreatedDateTimeUtc { get; set; }

    [Column("Creation Date")]
    public string CreatedDateView { get; set; } = string.Empty;

    [Column("Program Status")]
    public Status Status { get; set; }

    public void Register(TypeAdapterConfig config) =>
        config.NewConfig<TenantProgram, TenantProgramExportModel>()
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
            .Map(dest => dest.CreatedDateTimeUtc, src => src.Program.CreatedDateTimeUtc)
            .Map(dest => dest.Status, src => src.Status);
}
