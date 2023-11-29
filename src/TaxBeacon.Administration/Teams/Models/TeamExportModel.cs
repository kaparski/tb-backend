using Mapster;
using Npoi.Mapper.Attributes;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.Administration.Teams.Models;

public class TeamExportModel: IRegister
{
    [Column("Team Name")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Ignore]
    public DateTime CreatedDateTimeUtc { get; set; }

    [Column("Creation Date")]
    public string CreatedDateView { get; set; } = string.Empty;

    [Column("Number Of Users")]
    public int NumberOfUsers { get; set; }

    public void Register(TypeAdapterConfig config) =>
        config.NewConfig<Team, TeamExportModel>()
            .Map(dest => dest.NumberOfUsers, src =>
                src.Users.Count);
}
