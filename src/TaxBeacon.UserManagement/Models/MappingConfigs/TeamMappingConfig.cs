using Mapster;
using TaxBeacon.DAL.Entities;
using TaxBeacon.UserManagement.Models.Export;

namespace TaxBeacon.UserManagement.Models.MappingConfigs;

public class TeamMappingConfig: IRegister
{
    public void Register(TypeAdapterConfig config) =>
        config.NewConfig<Team, TeamExportModel>()
            .Map(dest => dest.NumberOfUsers, src =>
                src.Users.Count);
}
