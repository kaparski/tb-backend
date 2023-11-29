using Mapster;
using TaxBeacon.Common.Enums;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.Accounts.Contacts.Models;

public record ContactAccountDto: IRegister
{
    public Guid Id { get; init; }

    public string Name { get; init; } = null!;

    public string Type { get; init; } = null!;

    public Status Status { get; init; }

    public string? ClientState { get; init; }

    public Status? ClientStatus { get; init; }

    public string? ReferralState { get; init; }

    public Status? ReferralStatus { get; init; }

    public void Register(TypeAdapterConfig config) =>
        config.NewConfig<AccountContact, ContactAccountDto>()
            .Map(dest => dest.Id, src => src.AccountId)
            .Map(dest => dest.Name, src => src.Account != null ? src.Account.Name : null)
            .Map(dest => dest.ClientState, src => src.Account != null ? (src.Account.Client != null ? src.Account.Client.State : null) : null)
            .Map(dest => dest.ClientStatus,
                src =>  src.Account != null  ? (src.Account.Client != null ? (Status?)src.Account.Client.Status : null) : null)
            .Map(dest => dest.ReferralState, src =>  src.Account != null  ? (src.Account.Referral != null ? src.Account.Referral.State : null) : null)
            .Map(dest => dest.ReferralStatus, src => src.Account != null  ? (src.Account.Referral != null ? (Status?)src.Account.Referral.Status : null) : null);
}
