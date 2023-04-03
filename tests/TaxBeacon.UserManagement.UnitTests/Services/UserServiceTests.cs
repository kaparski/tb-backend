using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using Gridify;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net.Mail;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Exceptions;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL;
using TaxBeacon.DAL.Entities;
using TaxBeacon.DAL.Interceptors;
using TaxBeacon.DAL.Interfaces;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.UserManagement.UnitTests.Services;

public class UserServiceTests
{
    private readonly Mock<IDateTimeService> _dateTimeServiceMock;
    private readonly Mock<EntitySaveChangesInterceptor> _entitySaveChangesInterceptorMock;
    private readonly Mock<ILogger<UserService>> _userServiceLoggerMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IEnumerable<IListToFileConverter>> _listToFileConverters;
    private readonly Mock<IListToFileConverter> _csvMock;
    private readonly Mock<IListToFileConverter> _xlsxMock;
    private readonly Mock<IUserExternalStore> _userExternalStore;
    private readonly ITaxBeaconDbContext _dbContextMock;
    private readonly Mock<IDateTimeFormatter> _dateTimeFormatterMock;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userServiceLoggerMock = new();
        _entitySaveChangesInterceptorMock = new();
        _currentUserServiceMock = new();
        _dateTimeServiceMock = new();
        _listToFileConverters = new();
        _userExternalStore = new();
        _csvMock = new();
        _xlsxMock = new();
        _dateTimeFormatterMock = new();

        _csvMock.Setup(x => x.FileType).Returns(FileType.Csv);
        _xlsxMock.Setup(x => x.FileType).Returns(FileType.Xlsx);

        _listToFileConverters
            .Setup(x => x.GetEnumerator())
            .Returns((IEnumerator<IListToFileConverter>)new[] { _csvMock.Object, _xlsxMock.Object }.ToList()
                .GetEnumerator());

        _dbContextMock = new TaxBeaconDbContext(
            new DbContextOptionsBuilder<TaxBeaconDbContext>()
                .UseInMemoryDatabase($"{nameof(UserServiceTests)}-InMemoryDb-{Guid.NewGuid()}")
                .Options,
            _entitySaveChangesInterceptorMock.Object);

        var currentUser = TestData.TestUser.Generate();
        _dbContextMock.Users.Add(currentUser);

        _dateTimeServiceMock.SetupSequence(x => x.UtcNow)
            .Returns(DateTime.UtcNow)
            .Returns(DateTime.UtcNow);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUser.Id);

        _userService = new UserService(
            _userServiceLoggerMock.Object,
            _dbContextMock,
            _dateTimeServiceMock.Object,
            _currentUserServiceMock.Object,
            _listToFileConverters.Object,
            _userExternalStore.Object,
            _dateTimeFormatterMock.Object);

        TypeAdapterConfig.GlobalSettings.Scan(typeof(UserMappingConfig).Assembly);
    }

    [Fact]
    public async Task LoginAsync_ValidEmailAndUserExist_LastLoginDateUpdated()
    {
        //Arrange
        var tenant = TestData.TestTenant.Generate();
        var user = TestData.TestUser.Generate();
        var mailAddress = new MailAddress(user.Email);
        var currentDate = DateTime.UtcNow;

        user.TenantUsers.Add(new TenantUser { Tenant = tenant });
        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Users.AddAsync(user);
        await _dbContextMock.SaveChangesAsync();

        _dateTimeServiceMock
            .Setup(ds => ds.UtcNow)
            .Returns(DateTime.UtcNow);

        //Act
        var actualResult = await _userService.LoginAsync(mailAddress);

        //Assert
        (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
        actualResult.LastLoginDateTimeUtc.Should().BeAfter(currentDate);
        _dateTimeServiceMock
            .Verify(ds => ds.UtcNow, Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ValidEmailAndUserNotExist_NewUserCreated()
    {
        //Arrange
        var tenant = TestData.TestTenant.Generate();
        var user = TestData.TestUser.Generate();
        var mailAddress = new MailAddress(user.Email);
        var currentDate = DateTime.UtcNow;

        _dbContextMock.Tenants.Add(tenant);
        await _dbContextMock.SaveChangesAsync();

        _dateTimeServiceMock
            .Setup(s => s.UtcNow)
            .Returns(currentDate);

        //Act
        var actualResult = await _userService.LoginAsync(mailAddress);

        //Assert
        using (new AssertionScope())
        {
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            actualResult.Email.Should().Be(user.Email);
            actualResult.LastName.Should().BeEmpty();
            actualResult.FirstName.Should().BeEmpty();
            actualResult.LastLoginDateTimeUtc.Should().Be(currentDate);
            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Exactly(4));
        }
    }

    [Fact]
    public async Task GetUsersAsync_AscendingOrderingAndPaginationOfLastPage_AscendingOrderOfUsersAndCorrectPage()
    {
        // Arrange
        var users = TestData.TestUser.Generate(5);
        await _dbContextMock.Users.AddRangeAsync(users);
        await _dbContextMock.SaveChangesAsync();
        var query = new GridifyQuery { Page = 1, PageSize = 10, OrderBy = "email asc" };

        // Act
        var usersOneOf = await _userService.GetUsersAsync(query, default);

        // Assert
        usersOneOf.TryPickT0(out var pageOfUsers, out _);
        pageOfUsers.Should().NotBeNull();
        var listOfUsers = pageOfUsers.Query.ToList();
        listOfUsers.Count.Should().Be(6);
        listOfUsers.Select(x => x.Email).Should().BeInAscendingOrder();
        pageOfUsers.Count.Should().Be(6);
    }

    [Fact]
    public async Task GetUsersAsync_DescendingOrderingAndPaginationWithFirstPage_CorrectNumberOfUsersInDescendingOrder()
    {
        // Arrange
        var users = TestData.TestUser.Generate(6);
        await _dbContextMock.Users.AddRangeAsync(users);
        await _dbContextMock.SaveChangesAsync();
        var query = new GridifyQuery { Page = 1, PageSize = 4, OrderBy = "email desc" };

        // Act
        var usersOneOf = await _userService.GetUsersAsync(query, default);

        // Assert
        using (new AssertionScope())
        {
            usersOneOf.TryPickT0(out var pageOfUsers, out _);
            pageOfUsers.Should().NotBeNull();
            var listOfUsers = pageOfUsers.Query.ToList();
            listOfUsers.Count.Should().Be(4);
            listOfUsers.Select(x => x.Email).Should().BeInDescendingOrder();
            pageOfUsers.Count.Should().Be(7);
        }
    }

    [Fact]
    public async Task GetUsersAsync_PageNumberOutsideOfTotalRange_UserListIsEmpty()
    {
        // Arrange
        var users = TestData.TestUser.Generate(6);
        await _dbContextMock.Users.AddRangeAsync(users);
        await _dbContextMock.SaveChangesAsync();
        var query = new GridifyQuery { Page = 2, PageSize = 25, OrderBy = "email asc", };

        // Act
        var usersOneOf = await _userService.GetUsersAsync(query, default);

        // Assert
        usersOneOf.TryPickT0(out var pageOfUsers, out _);
        pageOfUsers.Should().BeNull();
    }

    [Fact]
    public async Task CreateUserAsync_ValidEmailAndUserNotExist_NewUserCreated()
    {
        //Arrange
        var faker = new Faker();
        var tenant = TestData.TestTenant.Generate();
        var user = new UserDto
        {
            FirstName = faker.Name.FirstName(),
            LastName = faker.Name.LastName(),
            Email = faker.Internet.Email()
        };

        _dbContextMock.Tenants.Add(tenant);
        await _dbContextMock.SaveChangesAsync();

        //Act
        await _userService.CreateUserAsync(user);
        var actualResult = await _dbContextMock.Users.LastAsync();

        //Assert
        using (new AssertionScope())
        {
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            actualResult.Email.Should().Be(user.Email);
            actualResult.LastName.Should().Be(user.LastName);
            actualResult.FirstName.Should().Be(user.FirstName);
            actualResult.TenantUsers.Should()
                .NotBeEmpty()
                .And
                .HaveCount(1);
            actualResult.TenantUsers.First().TenantId.Should().Be(tenant.Id);
            actualResult.CreatedDateTimeUtc.Should().Be(user.CreatedDateTimeUtc);
        }
    }

    [Fact]
    public async Task UpdateUserStatusAsync_ActiveUserStatusAndUserId_UpdatedUser()
    {
        //Arrange
        var tenant = TestData.TestTenant.Generate();
        var user = TestData.TestUser.Generate();
        var currentDate = DateTime.UtcNow;

        user.TenantUsers.Add(new TenantUser { Tenant = tenant });
        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Users.AddAsync(user);
        await _dbContextMock.SaveChangesAsync();

        _dateTimeServiceMock
            .Setup(ds => ds.UtcNow)
            .Returns(currentDate);

        //Act
        var actualResult = await _userService.UpdateUserStatusAsync(tenant.Id, user.Id, Status.Active);

        //Assert
        using (new AssertionScope())
        {
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            actualResult.Status.Should().Be(Status.Active);
            actualResult.DeactivationDateTimeUtc.Should().BeNull();
            actualResult.ReactivationDateTimeUtc.Should().Be(currentDate);

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Exactly(3));
        }
    }

    [Fact]
    public async Task UpdateUserStatusAsync_DeactivatedUserStatusAndUserId_UpdatedUser()
    {
        //Arrange
        var tenant = TestData.TestTenant.Generate();
        var user = TestData.TestUser.Generate();
        var currentDate = DateTime.UtcNow;

        user.TenantUsers.Add(new TenantUser { Tenant = tenant });
        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Users.AddAsync(user);
        await _dbContextMock.SaveChangesAsync();

        _dateTimeServiceMock
            .Setup(ds => ds.UtcNow)
            .Returns(currentDate);

        //Act
        var actualResult = await _userService.UpdateUserStatusAsync(tenant.Id, user.Id, Status.Deactivated);

        //Assert
        using (new AssertionScope())
        {
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            actualResult.Status.Should().Be(Status.Deactivated);
            actualResult.ReactivationDateTimeUtc.Should().BeNull();
            actualResult.DeactivationDateTimeUtc.Should().Be(currentDate);

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Exactly(4));

            // TODO: Verify logs
        }
    }

    [Theory]
    [MemberData(nameof(TestData.UpdatedStatusInvalidData), MemberType = typeof(TestData))]
    public async Task UpdateUserStatusAsync_UserStatusAndUserIdNotInDb_ThrowNotFoundException(Status status,
        Guid userId)
    {
        //Act
        Func<Task> act = async () => await _userService.UpdateUserStatusAsync(Guid.NewGuid(), userId, status);

        //Assert
        await act
            .Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"Entity \"{nameof(User)}\" ({userId}) was not found.");
    }

    [Fact]
    public async Task GetUserByEmailAsync_ValidUserEmail_ReturnsUser()
    {
        //Arrange
        var tenant = TestData.TestTenant.Generate();
        var users = TestData.TestUser.Generate(5);
        var userEmail = users.First().Email;

        foreach (var user in users)
        {
            user.TenantUsers.Add(new TenantUser { Tenant = tenant });
        }

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Users.AddRangeAsync(users);
        await _dbContextMock.SaveChangesAsync();

        //Act
        var actualResult = await _userService.GetUserByEmailAsync(new MailAddress(userEmail));

        //Assert
        actualResult.Email.Should().Be(userEmail);
    }

    [Fact]
    public async Task GetUserByEmailAsync_UserEmailNotInDb_ReturnsUser()
    {
        //Arrange
        var tenant = TestData.TestTenant.Generate();
        var users = TestData.TestUser.Generate(5);
        var email = new Faker().Internet.Email();

        foreach (var user in users)
        {
            user.TenantUsers.Add(new TenantUser { Tenant = tenant });
        }

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Users.AddRangeAsync(users);
        await _dbContextMock.SaveChangesAsync();

        //Act
        Func<Task> act = async () => await _userService.GetUserByEmailAsync(new MailAddress(email));

        //Assert
        await act
            .Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"Entity \"{nameof(User)}\" ({email}) was not found.");
    }

    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public async Task ExportUsersAsync_ValidInputData_AppropriateConverterShouldBeCalled(FileType fileType)
    {
        //Arrange
        var tenant = TestData.TestTenant.Generate();
        var users = TestData.TestUser.Generate(5);

        foreach (var user in users)
        {
            user.TenantUsers.Add(new TenantUser { Tenant = tenant });
        }

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Users.AddRangeAsync(users);
        await _dbContextMock.SaveChangesAsync();

        //Act
        _ = await _userService.ExportUsersAsync(tenant.Id, fileType, default);

        //Assert
        if (fileType == FileType.Csv)
        {
            _csvMock.Verify(x => x.Convert(It.IsAny<List<UserExportModel>>()), Times.Once());
        }
        else if (fileType == FileType.Xlsx)
        {
            _xlsxMock.Verify(x => x.Convert(It.IsAny<List<UserExportModel>>()), Times.Once());
        }
        else
        {
            throw new InvalidOperationException();
        }
    }

    [Fact]
    public async Task UpdateUserByIdAsync_InvalidUserId_ReturnsNotFound()
    {
        // Arrange
        var updateUserDto = TestData.UpdateUserDtoFaker.Generate();
        var user = TestData.TestUser.Generate();
        var tenant = TestData.TestTenant.Generate();
        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Users.AddAsync(user);
        await _dbContextMock.SaveChangesAsync();

        // Act
        var usersOneOf = await _userService.UpdateUserByIdAsync(Guid.Empty, Guid.NewGuid(), updateUserDto);

        // Assert
        using (new AssertionScope())
        {
            usersOneOf.TryPickT0(out var userDto, out _);
            userDto.Should().BeNull();
        }
    }

    [Fact]
    public async Task UpdateUserByIdAsync_ValidUserIdAndUpdateUserDto_ReturnsUpdatedUser()
    {
        // Arrange
        var updateUserDto = TestData.UpdateUserDtoFaker.Generate();
        var user = TestData.TestUser.Generate();
        var oldFirstName = user.FirstName;
        var oldLastName = user.LastName;
        var tenant = TestData.TestTenant.Generate();
        var currentDate = DateTime.UtcNow;

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Users.AddAsync(user);
        await _dbContextMock.TenantUsers.AddAsync(new TenantUser
        {
            Tenant = tenant,
            User = user
        });

        await _dbContextMock.SaveChangesAsync();

        _dateTimeServiceMock
            .Setup(service => service.UtcNow)
            .Returns(currentDate);

        // Act
        var usersOneOf = await _userService.UpdateUserByIdAsync(Guid.Empty, user.Id, updateUserDto);

        // Assert
        using (new AssertionScope())
        {
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            usersOneOf.TryPickT0(out var userDto, out _);
            userDto.Should().NotBeNull();
            userDto.Id.Should().Be(user.Id);
            userDto.FirstName.Should().Be(updateUserDto.FirstName);
            userDto.FirstName.Should().NotBe(oldFirstName);
            userDto.LastName.Should().Be(updateUserDto.LastName);
            userDto.LastName.Should().NotBe(oldLastName);

            var actualActivityLog = await _dbContextMock.UserActivityLogs.LastOrDefaultAsync();
            actualActivityLog.Should().NotBeNull();
            actualActivityLog?.Date.Should().Be(currentDate);
            actualActivityLog?.EventType.Should().Be(EventType.UserUpdated);
            actualActivityLog?.UserId.Should().Be(user.Id);
            actualActivityLog?.TenantId.Should().Be(tenant.Id);

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Once);
        }
    }

    [Fact]
    public async Task AssignRoleAsync_ValidRoleIds_ShouldAssignAllProvidedRoles()
    {
        //Arrange
        var tenant = TestData.TestTenant.Generate();
        var user = TestData.TestUser.Generate();
        var roles = TestData.TestRoles.Generate(2);
        var tenantUser = new TenantUser
        {
            Tenant = tenant,
            User = user
        };

        user.TenantUsers.Add(tenantUser);
        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Users.AddAsync(user);
        await _dbContextMock.SaveChangesAsync();

        //Act
        await _userService.AssignRoleAsync(tenant.Id, roles.Select(x => x.Id).ToArray(), user.Id, default);

        //Assert
        _dbContextMock.TenantUserRoles.Count().Should().Be(2);
    }

    [Fact]
    public async Task AssignRoleAsync_ValidRoleIds_ShouldAssignAllProvidedRolesAndRemoveNotAssigned()
    {
        //Arrange
        var tenant = TestData.TestTenant.Generate();
        var user = TestData.TestUser.Generate();
        var roles = TestData.TestRoles.Generate(2);
        var tenantRoles = roles.Select(x => new TenantRole
        {
            Tenant = tenant,
            Role = x
        });
        var tenantUser = new TenantUser
        {
            Tenant = tenant,
            User = user
        };

        foreach (var tenantRole in tenantRoles)
        {
            await _dbContextMock.TenantUserRoles.AddAsync(new TenantUserRole()
            {
                TenantUser = tenantUser,
                TenantRole = tenantRole
            });
        }
        user.TenantUsers.Add(tenantUser);
        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Users.AddAsync(user);
        await _dbContextMock.SaveChangesAsync();

        //Act
        await _userService.AssignRoleAsync(tenant.Id, new[] { roles.Select(x => x.Id).First() }, user.Id, default);

        //Assert
        _dbContextMock.TenantUserRoles.Count().Should().Be(1);
    }

    private static class TestData
    {
        public static readonly Faker<Tenant> TestTenant =
            new Faker<Tenant>()
                .RuleFor(t => t.Id, f => Guid.NewGuid())
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.CreatedDateTimeUtc, f => DateTime.UtcNow);

        public static readonly Faker<User> TestUser =
            new Faker<User>()
                .RuleFor(u => u.Id, f => Guid.NewGuid())
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.FullName, (_, u) => $"{u.FirstName} {u.LastName}")
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.CreatedDateTimeUtc, f => DateTime.UtcNow)
                .RuleFor(u => u.Status, f => f.PickRandom<Status>());

        public static readonly Faker<UpdateUserDto> UpdateUserDtoFaker =
            new Faker<UpdateUserDto>()
                .RuleFor(dto => dto.FirstName, f => f.Name.FirstName())
                .RuleFor(dto => dto.LastName, f => f.Name.LastName());

        public static IEnumerable<object[]> UpdatedStatusInvalidData =>
            new List<object[]>
            {
                new object[] { Status.Active, Guid.NewGuid() },
                new object[] { Status.Deactivated, Guid.Empty }
            };

        public static readonly Faker<Role> TestRoles =
            new Faker<Role>()
                .RuleFor(u => u.Id, f => Guid.NewGuid())
                .RuleFor(u => u.Name, f => f.Name.JobTitle());
    }
}
