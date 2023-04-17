using Npoi.Mapper.Attributes;
using System.ComponentModel.DataAnnotations;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.UserManagement.Models
{
    public sealed class DepartmentExportModel
    {
        [Column("Department Name")]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string? Division { get; set; }

        [Ignore]
        public DateTime CreatedDateTimeUtc { get; set; }

        [Column("Creation date")]
        public string CreatedDateView { get; set; } = string.Empty;

        [Column("Number of Users")]
        public int AssignedUsersCount { get; set; }
    }
}
