using Mapster;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.Accounts.Common.Models;
public record NaicsCodeDto: IRegister
{
    public int Code { get; init; }

    public string Title { get; init; } = null!;

    public void Register(TypeAdapterConfig config) => config.NewConfig<NaicsCode, NaicsCodeDto>().MaxDepth(1);
}
