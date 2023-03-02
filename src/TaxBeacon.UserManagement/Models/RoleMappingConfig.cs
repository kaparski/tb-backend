using Mapster;
using TaxBeacon.DAL.Entities;

namespace TaxBeacon.UserManagement.Models;

public class RoleMappingConfig: IRegister
{
    public void Register(TypeAdapterConfig config) => config.NewConfig<Role, RoleDto>()
        .Map(dest => dest.NumberOfUsers, src =>
            src.TenantRoles.FirstOrDefault()!.TenantUserRoles.Count);
}

