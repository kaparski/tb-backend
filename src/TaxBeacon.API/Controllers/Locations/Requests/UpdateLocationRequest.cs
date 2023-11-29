using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxBeacon.Accounts.Naics;
using TaxBeacon.API.Extensions;
using TaxBeacon.API.Shared.Requests;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Accounts;

namespace TaxBeacon.API.Controllers.Locations.Requests;

public record UpdateLocationRequest: ILocationRequest, INaicsCodeRequest
{
    public string Name { get; init; } = null!;

    public Country Country { get; init; } = null!;

    public string? Address1 { get; init; }

    public string? Address2 { get; init; }

    public string? City { get; init; }

    public State? State { get; init; }

    public string? Zip { get; init; }

    public string? County { get; init; }

    public string? Address { get; init; }

    public int? PrimaryNaicsCode { get; init; }

    public DateTime? StartDateTimeUtc { get; init; }

    public DateTime? EndDateTimeUtc { get; init; }

    public string LocationId { get; init; } = null!;

    public LocationType Type { get; init; }

    public IEnumerable<CreateUpdatePhoneRequest> Phones { get; init; } = Enumerable.Empty<CreateUpdatePhoneRequest>();
}

public sealed class UpdateLocationRequestValidator: AbstractValidator<UpdateLocationRequest>
{
    public UpdateLocationRequestValidator(IHttpContextAccessor httpContextAccessor, IAccountDbContext dbContext,
        ICurrentUserService currentUserService, INaicsService naicsService)
    {
        RuleFor(l => l.Name)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("The location name must contain no more than {MaxLength} characters")
            .MustAsync(async (name, cancellationToken) =>
                !httpContextAccessor.HttpContext.TryGetIdFromRoute("locationId", out var id)
                || !httpContextAccessor.HttpContext.TryGetIdFromRoute("accountId", out var accountId)
                || !await dbContext.Locations.AnyAsync(l => l.Name == name
                                                            && l.AccountId == accountId
                                                            && id != l.Id,
                    cancellationToken))
            .WithMessage("Location with the same name already exists");

        RuleFor(l => l.LocationId)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("The location id must contain no more than {MaxLength} characters")
            .MustAsync(async (locationId, cancellationToken) =>
                !httpContextAccessor.HttpContext.TryGetIdFromRoute("locationId", out var id)
                || !await dbContext.Locations.AnyAsync(l => l.LocationId == locationId
                                                            && l.TenantId == currentUserService.TenantId
                                                            && id != l.Id,
                    cancellationToken))
            .WithMessage("Location with the same Location ID already exists");

        RuleFor(x => x.EndDateTimeUtc)
            .GreaterThanOrEqualTo(x => x.StartDateTimeUtc)
            .When(x => x.StartDateTimeUtc is not null && x.EndDateTimeUtc is not null,
                ApplyConditionTo.CurrentValidator)
            .WithMessage("The location end date must be greater than or equal to the location start date");

        Include(new LocationRequestValidation());

        Include(new NaicsCodeRequestValidation(naicsService));
    }
}
