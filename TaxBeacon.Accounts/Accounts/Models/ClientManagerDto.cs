using Mapster;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.Accounts.Accounts.Models;
public record ClientManagerDto: IRegister
{
    public Guid UserId { get; init; }
    public Guid TenantId { get; init; }
    public string FullName { get; set; } = null!;

    public void Register(TypeAdapterConfig config) =>
       config.NewConfig<ClientManager, ClientManagerDto>()
           .Map(dest => dest.FullName, src =>
               src.TenantUser.User.FullName);
}
