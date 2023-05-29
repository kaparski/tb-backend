﻿using Mapster;
using TaxBeacon.Common.Enums;
using TaxBeacon.DAL.Entities;

namespace TaxBeacon.UserManagement.Services.Program.Models;

public class TenantProgramDto: IRegister
{
    public Guid Id { get; set; }

    public string Reference { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Overview { get; set; } = string.Empty;

    public string LegalAuthority { get; set; } = string.Empty;

    public string Agency { get; set; } = string.Empty;

    public Jurisdiction Jurisdiction { get; set; }

    public string JurisdictionName { get; set; } = string.Empty;

    public string IncentivesArea { get; set; } = string.Empty;

    public string IncentivesType { get; set; } = string.Empty;

    public Status Status { get; set; }

    public DateTime StartDateTimeUtc { get; set; }

    public DateTime EndDateTimeUtc { get; set; }

    public DateTime CreatedDateTimeUtc { get; set; }

    public DateTime? DeactivationDateTimeUtc { get; set; }

    public DateTime? ReactivationDateTimeUtc { get; set; }

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
            .Map(dest => dest.CreatedDateTimeUtc, src => src.Program.CreatedDateTimeUtc)
            .Map(dest => dest.DeactivationDateTimeUtc, src => src.DeactivationDateTimeUtc)
            .Map(dest => dest.ReactivationDateTimeUtc, src => src.ReactivationDateTimeUtc)
            .Map(dest => dest.Status, src => src.Status);
}
