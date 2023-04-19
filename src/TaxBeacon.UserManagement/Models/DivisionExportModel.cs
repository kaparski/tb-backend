using Npoi.Mapper.Attributes;
using System.ComponentModel.DataAnnotations;

namespace TaxBeacon.UserManagement.Models
{
    public sealed class DivisionExportModel
    {

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Departments { get; set; } = string.Empty;

        [Column("Number of users")]
        public int NumberOfUsers { get; set; }

        [Ignore]
        public DateTime CreatedDateTimeUtc { get; set; }

        [Column("Creation date")]
        public string CreatedDateView { get; set; } = string.Empty;
    }
}
