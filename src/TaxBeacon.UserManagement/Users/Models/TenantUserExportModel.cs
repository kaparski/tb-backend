using Mapster;
using Npoi.Mapper.Attributes;
using System.ComponentModel.DataAnnotations;
using TaxBeacon.Common.Enums;
using TaxBeacon.DAL.Entities;

namespace TaxBeacon.UserManagement.Users.Models;

public sealed class TenantUserExportModel: IRegister
{
    public string Email { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    [Display(Name = "Job Title")]
    [Column("Job Title")]
    public string JobTitle { get; set; } = string.Empty;

    public string Roles { get; set; } = string.Empty;

    [Ignore]
    public DateTime? LastLoginDateTimeUtc { get; set; }

    [Column("Last Login")]
    public string LastLoginDateView { get; set; } = string.Empty;

    [Display(Name = "Status")]
    [Column("Status")]
    public Status Status { get; set; }

    [Ignore]
    public DateTime CreatedDateTimeUtc { get; set; }

    [Column("Creation date")]
    public string CreatedDateView { get; set; } = string.Empty;

    [Ignore]
    public DateTime? DeactivationDateTimeUtc { get; set; }

    [Column("Deactivation date")]
    public string DeactivationDateTimeView { get; set; } = string.Empty;

    [Ignore]
    public DateTime? ReactivationDateTimeUtc { get; set; }

    [Column("Reactivation date")]
    public string ReactivationDateTimeView { get; set; } = string.Empty;

    public void Register(TypeAdapterConfig config) =>
        config.NewConfig<User, TenantUserExportModel>()
            .Map(dest => dest.Roles, src =>
                src.TenantUsers
                    .SelectMany(tu => tu.TenantUserRoles.Select(tur => tur.TenantRole.Role.Name))
                    .GroupBy(r => 1, name => name)
                    .Select(group => string.Join(", ", group.Select(name => name)))
                    .FirstOrDefault());
}