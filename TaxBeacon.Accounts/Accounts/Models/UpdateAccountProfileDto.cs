using Mapster;
using TaxBeacon.Accounts.Common.Models;
using TaxBeacon.Common.Enums;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.Accounts.Accounts.Models;

public record UpdateAccountProfileDto: IRegister
{
    public string Name { get; init; } = null!;

    public string? DoingBusinessAs { get; init; }

    public string? LinkedInUrl { get; init; }

    public string Website { get; init; } = null!;

    public string Country { get; init; } = null!;

    public string? Address1 { get; init; }

    public string? Address2 { get; init; }

    public string? City { get; init; }

    public State? State { get; init; }

    public string? Zip { get; init; }

    public string? County { get; init; }

    public string? Address { get; init; }

    public int? PrimaryNaicsCode { get; init; }

    public string AccountId { get; init; } = null!;

    public IEnumerable<Guid> SalespersonIds { get; init; } = Enumerable.Empty<Guid>();

    public IEnumerable<CreateUpdatePhoneDto> Phones { get; init; } = Enumerable.Empty<CreateUpdatePhoneDto>();

    public void Register(TypeAdapterConfig config) =>
        config.NewConfig<UpdateAccountProfileDto, Account>().Ignore(x => x.Phones);

    public virtual bool Equals(UpdateAccountProfileDto? updateAccountProfileDto) => updateAccountProfileDto is not null
            && Name == updateAccountProfileDto.Name
            && DoingBusinessAs == updateAccountProfileDto.DoingBusinessAs
            && LinkedInUrl == updateAccountProfileDto.LinkedInUrl
            && Website == updateAccountProfileDto.Website
            && Country == updateAccountProfileDto.Country
            && Address == updateAccountProfileDto.Address
            && Address1 == updateAccountProfileDto.Address1
            && Address2 == updateAccountProfileDto.Address2
            && State == updateAccountProfileDto.State
            && City == updateAccountProfileDto.City
            && County == updateAccountProfileDto.County
            && Zip == updateAccountProfileDto.Zip
            && PrimaryNaicsCode == updateAccountProfileDto.PrimaryNaicsCode
            && AccountId == updateAccountProfileDto.AccountId
            && Phones.SequenceEqual(updateAccountProfileDto.Phones);

    public override int GetHashCode() => base.GetHashCode();
}
