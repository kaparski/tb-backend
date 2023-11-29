using Bogus;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaxBeacon.Accounts.Naics;
using TaxBeacon.Accounts.UnitTests.Accounts;
using TaxBeacon.DAL;
using TaxBeacon.DAL.Accounts.Entities;
using TaxBeacon.DAL.Interceptors;

namespace TaxBeacon.Accounts.UnitTests.Naics;
public partial class NaicsServiceTests
{
    private readonly TaxBeaconDbContext _dbContextMock;
    private readonly Mock<EntitySaveChangesInterceptor> _entitySaveChangesInterceptorMock;
    private readonly INaicsService _naicsService;
    public NaicsServiceTests()
    {
        _entitySaveChangesInterceptorMock = new();
        _dbContextMock = new TaxBeaconDbContext(
            new DbContextOptionsBuilder<TaxBeaconDbContext>()
                .UseInMemoryDatabase($"{nameof(AccountsServiceTests)}-InMemoryDb-{Guid.NewGuid()}")
                .Options,
            _entitySaveChangesInterceptorMock.Object);

        _naicsService = new NaicsService(_dbContextMock);
    }

    private static class TestData
    {
        public static readonly Faker<NaicsCode> NaicsCodeFaker =
            new Faker<NaicsCode>()
            .RuleFor(nc => nc.Title, f => f.Commerce.Product());
    }
}
