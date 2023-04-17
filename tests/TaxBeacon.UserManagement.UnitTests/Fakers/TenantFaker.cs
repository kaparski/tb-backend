using Bogus;
using TaxBeacon.DAL.Entities;

namespace TaxBeacon.UserManagement.UnitTests.Fakers;

public class TenantFaker: Faker<Tenant>
{

    public TenantFaker() =>
        UseSeed(1969)
            .RuleFor(t => t.Id, f => f.Random.Guid())
            .RuleFor(t => t.Name, f => f.Company.CompanyName())
            .RuleFor(t => t.CreatedDateTimeUtc, _ => DateTime.UtcNow);

}
