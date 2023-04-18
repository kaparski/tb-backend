using Npoi.Mapper.Attributes;
using System.ComponentModel.DataAnnotations;

namespace TaxBeacon.UserManagement.Models
{
    public sealed class DivisionExportModel
    {
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Status")]

        [Ignore]
        public DateTime CreatedDateTimeUtc { get; set; }

        [Column("Creation date")]
        public string CreatedDateView { get; set; } = string.Empty;
    }
}
