using Mapster;
using TaxBeacon.DAL.Entities;

namespace TaxBeacon.UserManagement.Models.MappingConfigs;

public class DivisionMappingConfig: IRegister
{
    public void Register(TypeAdapterConfig config) =>
        config.NewConfig<User, DivisionUserDto>()
            .Map(dest => dest.Department, src =>
                src.Department == null ? string.Empty : src.Department.Name)
            .Map(dest => dest.JobTitle, src =>
                src.JobTitle == null ? string.Empty : src.JobTitle.Name);
}
