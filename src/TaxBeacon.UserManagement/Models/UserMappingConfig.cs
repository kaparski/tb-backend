using Mapster;
using TaxBeacon.DAL.Entities;

namespace TaxBeacon.UserManagement.Models;

public class UserMappingConfig: IRegister
{
    public void Register(TypeAdapterConfig config) =>
        config.NewConfig<User, UserDto>()
            .Map(dest => dest.Roles, src =>
                src.TenantUsers
                    .SelectMany(tu => tu.TenantUserRoles.Select(tur => tur.TenantRole.Role.Name))
                    .GroupBy(r => 1, name => name)
                    .Select(group => string.Join(", ", group.Select(name => name)))
                    .FirstOrDefault());
}
