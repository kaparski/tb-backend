using Npoi.Mapper.Attributes;
using System.ComponentModel.DataAnnotations;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.UserManagement.Models
{
    public sealed class UserExportModel
    {
        public string Email { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;

        [Display(Name = "Job Title")]
        [Column("Job Title")]
        public string JobTitle { get; set; } = string.Empty;

        public string Roles { get; set; } = string.Empty;

        [Display(Name = "Last Login")]
        [Column("Last Login", CustomFormat = "MM.dd.yyyy hh:mm:ss AM/PM")]
        public DateTime? LastLoginDateUtc { get; set; }

        [Display(Name = "Status")]
        [Column("Status")]
        public UserStatus UserStatus { get; set; }

        [Display(Name = "Creation date")]
        [Column("Creation date", CustomFormat = "MM.dd.yyyy hh:mm:ss AM/PM")]
        public DateTime CreatedDateUtc { get; set; }

        [Display(Name = "Deactivation date")]
        [Column("Deactivation date", CustomFormat = "MM.dd.yyyy hh:mm:ss AM/PM")]
        public DateTime? DeactivationDateTimeUtc { get; set; }

        [Display(Name = "Reactivation date")]
        [Column("Reactivation date", CustomFormat = "MM.dd.yyyy hh:mm:ss AM/PM")]
        public DateTime? ReactivationDateTimeUtc { get; set; }
    }
}
