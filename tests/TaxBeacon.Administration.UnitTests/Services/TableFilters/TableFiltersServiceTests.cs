using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TaxBeacon.Administration.TableFilters;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Administration;
using TaxBeacon.DAL.Interceptors;
using TaxBeacon.DAL;
using Bogus;
using TaxBeacon.Administration.TableFilters.Models;
using TaxBeacon.DAL.Administration.Entities;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.Administration.UnitTests.Services.TableFilters;
public partial class TableFiltersServiceTests
{
    private readonly Mock<IDateTimeService> _dateTimeServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly ITaxBeaconDbContext _dbContextMock;
    private readonly ITableFiltersService _tableFiltersService;

    public TableFiltersServiceTests()
    {
        Mock<EntitySaveChangesInterceptor> entitySaveChangesInterceptorMock = new();
        Mock<ILogger<TableFilterService>> tableFiltersServiceLoggerMock = new();

        _dateTimeServiceMock = new();
        _currentUserServiceMock = new();

        _dbContextMock = new TaxBeaconDbContext(
            new DbContextOptionsBuilder<TaxBeaconDbContext>()
                .UseInMemoryDatabase($"{nameof(TableFiltersServiceTests)}-InMemoryDb-{Guid.NewGuid()}")
                .Options,
            entitySaveChangesInterceptorMock.Object);

        _tableFiltersService = new TableFilterService(
            tableFiltersServiceLoggerMock.Object,
            _dbContextMock,
            _dateTimeServiceMock.Object,
            _currentUserServiceMock.Object);
    }

    private static class TestData
    {
        public static readonly Faker<Tenant> TenantFaker =
            new Faker<Tenant>()
                .RuleFor(t => t.Id, _ => Guid.NewGuid())
                .RuleFor(t => t.Name, f => f.Company.CompanyName());

        public static readonly Faker<TableFilter> TableFilterFaker =
            new Faker<TableFilter>()
                .RuleFor(tf => tf.Id, _ => Guid.NewGuid())
                .RuleFor(tf => tf.Name, f => f.Hacker.Noun())
                .RuleFor(tf => tf.Configuration, f => f.Lorem.Text())
                .RuleFor(tf => tf.TableType, f => f.PickRandom<EntityType>())
                .RuleFor(tf => tf.TenantId, _ => Guid.NewGuid())
                .RuleFor(tf => tf.UserId, _ => Guid.NewGuid());

        public static readonly Faker<User> UserFaker =
            new Faker<User>()
                .RuleFor(u => u.Id, f => Guid.NewGuid())
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.FullName, f => f.Name.FullName())
                .RuleFor(u => u.LegalName, (_, u) => u.FirstName)
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.CreatedDateTimeUtc, f => DateTime.UtcNow)
                .RuleFor(u => u.Status, f => f.PickRandom<Status>());

        public static readonly Faker<CreateTableFilterDto> CreateTableFilterDtoFaker =
            new Faker<CreateTableFilterDto>()
                .RuleFor(dto => dto.Name, f => f.Hacker.Noun())
                .RuleFor(dto => dto.Configuration, f => f.Lorem.Text())
                .RuleFor(dto => dto.TableType, f => f.PickRandom<EntityType>());
    }
}
