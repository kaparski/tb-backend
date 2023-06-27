using Mapster;
using TaxBeacon.DAL.Entities.Accounts;

namespace TaxBeacon.Accounts.Accounts.Models;
public record ClientManagerDto
{
    public Guid UserId { get; init; }
    public Guid TenantId { get; init; }
    public string FullName { get; set; } = null!;
}

public class ClientManagerMappingConfig: IRegister
{
    public void Register(TypeAdapterConfig config) =>
        config.NewConfig<ClientManager, ClientManagerDto>()
            .Map(dest => dest.FullName, src =>
                src.User.User.FullName);
}
