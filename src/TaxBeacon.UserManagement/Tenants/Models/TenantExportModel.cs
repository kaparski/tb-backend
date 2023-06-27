using Npoi.Mapper.Attributes;
using System.ComponentModel.DataAnnotations;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.UserManagement.Tenants.Models;

public sealed class TenantExportModel
{
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Status")]
    [Column("Status")]
    public Status Status { get; set; }

    [Ignore]
    public DateTime CreatedDateTimeUtc { get; set; }

    [Column("Creation date")]
    public string CreatedDateView { get; set; } = string.Empty;
}