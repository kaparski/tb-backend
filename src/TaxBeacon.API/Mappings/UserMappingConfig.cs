using Mapster;
using TaxBeacon.API.Controllers.Users.Responses;
using TaxBeacon.Administration.Users.Models;

namespace TaxBeacon.API.Mappings;

public class UserMappingConfig: IRegister
{
    public void Register(TypeAdapterConfig config) => config.NewConfig<UserDto, UserResponse>()
            .Map(
                dest => dest.Roles,
                src => src.Roles != null ? src.Roles.Replace("|", string.Empty) : null
            );
}
