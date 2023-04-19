using Mapster;
using TaxBeacon.DAL.Entities;

namespace TaxBeacon.UserManagement.Models.MappingConfigs
{
    public class DivisionMappingConfig: IRegister
    {
        public void Register(TypeAdapterConfig config) =>
        config.NewConfig<Division, DivisionExportModel>()
            .Map(dest => dest.Departments, src => string.Join(", ", src.Departments.Select(d => d.Name)))
            .Map(dest => dest.NumberOfUsers, src =>
                src.Users.Count);
    }
}
