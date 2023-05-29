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
using TaxBeacon.Common.Services;
using TaxBeacon.DAL;
using TaxBeacon.DAL.Entities;
using TaxBeacon.DAL.Interceptors;
using TaxBeacon.DAL.Interfaces;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Models.Activities;
using TaxBeacon.UserManagement.Models.Export;
using TaxBeacon.UserManagement.Services;
using TaxBeacon.UserManagement.Services.Activities;
using JsonSerializer = System.Text.Json.JsonSerializer;

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
    private readonly Mock<IUserActivityFactory> _userCreatedActivityFactory;
    private readonly Mock<IEnumerable<IUserActivityFactory>> _activityFactories;
    private readonly Guid _tenantId = Guid.NewGuid();

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
        _userCreatedActivityFactory = new();
        _activityFactories = new();

        _csvMock.Setup(x => x.FileType).Returns(FileType.Csv);
        _xlsxMock.Setup(x => x.FileType).Returns(FileType.Xlsx);

        _userCreatedActivityFactory.Setup(x => x.UserEventType).Returns(UserEventType.UserCreated);
        _userCreatedActivityFactory.Setup(x => x.Revision).Returns(1);

        _listToFileConverters
            .Setup(x => x.GetEnumerator())
            .Returns(new[] { _csvMock.Object, _xlsxMock.Object }.ToList()
                .GetEnumerator());

        _activityFactories
            .Setup(x => x.GetEnumerator())
            .Returns(new[] { _userCreatedActivityFactory.Object }.ToList().GetEnumerator());

        _dbContextMock = new TaxBeaconDbContext(
            new DbContextOptionsBuilder<TaxBeaconDbContext>()
                .UseInMemoryDatabase($"{nameof(UserServiceTests)}-InMemoryDb-{Guid.NewGuid()}")
                .Options,
            _entitySaveChangesInterceptorMock.Object);

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(_tenantId);

        _userService = new UserService(
            _userServiceLoggerMock.Object,
            _dbContextMock,
            _dateTimeServiceMock.Object,
            _currentUserServiceMock.Object,
            _listToFileConverters.Object,
            _userExternalStore.Object,
            _dateTimeFormatterMock.Object,
            _activityFactories.Object);

        TypeAdapterConfig.GlobalSettings.Scan(typeof(IUserService).Assembly);
    }

    [Fact]
    public async Task LoginAsync_ValidEmailAndUserExist_ReturnsLoginUserDto()
    {
        //Arrange
        var user = TestData.TestUser.Generate();
        var mailAddress = new MailAddress(user.Email);
        var currentDate = DateTime.UtcNow;

        await _dbContextMock.Users.AddAsync(user);
        await _dbContextMock.SaveChangesAsync();

        _dateTimeServiceMock
            .Setup(ds => ds.UtcNow)
            .Returns(currentDate);

        //Act
        var actualResultOneOf = await _userService.LoginAsync(mailAddress);

        //Assert
        (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
        var actualUser = await _dbContextMock.Users.LastAsync();
        actualUser.LastLoginDateTimeUtc.Should().Be(currentDate);

        actualResultOneOf.TryPickT0(out var loginUserDto, out _).Should().BeTrue();
        loginUserDto.UserId.Should().Be(user.Id);
        loginUserDto.FullName.Should().Be(user.FullName);
        loginUserDto.Permissions.Should().BeEquivalentTo(Enumerable.Empty<string>());
        loginUserDto.IsSuperAdmin.Should().BeFalse();

        _dateTimeServiceMock
            .Verify(ds => ds.UtcNow, Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ValidEmailAndUserExist_ReturnsSuperAdminLoginUserDto()
    {
        //Arrange
        var tenant = TestData.TestTenant.Generate();
        var user = TestData.TestUser.Generate();
        var supeAdminRole = new Role
        {
            Id = Guid.NewGuid(),
            Name = "Super admin"
        };
        var mailAddress = new MailAddress(user.Email);
        var currentDate = DateTime.UtcNow;

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Users.AddAsync(user);
        await _dbContextMock.Roles.AddAsync(supeAdminRole);
        await _dbContextMock.UserRoles.AddAsync(new UserRole { UserId = user.Id, RoleId = supeAdminRole.Id });
        await _dbContextMock.SaveChangesAsync();

        _dateTimeServiceMock
            .Setup(ds => ds.UtcNow)
            .Returns(currentDate);

        //Act
        var actualResultOneOf = await _userService.LoginAsync(mailAddress);

        //Assert
        using (new AssertionScope())
        {
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            var actualUser = await _dbContextMock.Users.FirstAsync(u => u.Id == user.Id);
            actualUser.LastLoginDateTimeUtc.Should().Be(currentDate);

            actualResultOneOf.TryPickT0(out var loginUserDto, out _).Should().BeTrue();
            loginUserDto.UserId.Should().Be(user.Id);
            loginUserDto.FullName.Should().Be(user.FullName);
            loginUserDto.Permissions.Should().BeEquivalentTo(Enumerable.Empty<string>());
            loginUserDto.IsSuperAdmin.Should().BeTrue();

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Once);
        }
    }

    [Fact]
    public async Task LoginAsync_ValidEmailAndUserNotExist_ReturnsNotFound()
    {
        //Arrange
        var mailAddress = new MailAddress(new Faker().Internet.Email());
        var currentDate = DateTime.UtcNow;

        _dateTimeServiceMock
            .Setup(s => s.UtcNow)
            .Returns(currentDate);

        //Act
        var actualResultOneOf = await _userService.LoginAsync(mailAddress);

        //Assert
        using (new AssertionScope())
        {
            actualResultOneOf.IsT0.Should().BeFalse();
            actualResultOneOf.IsT1.Should().BeTrue();
            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Never);
        }
    }

    [Fact]
    public async Task GetUsersAsync_AscendingOrderingAndPaginationOfLastPage_AscendingOrderOfUsersAndCorrectPage()
    {
        // Arrange
        var users = TestData.TestUser.Generate(5);
        var tenant = TestData.TestTenant.Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Users.AddRangeAsync(users);
        await _dbContextMock.TenantUsers.AddRangeAsync(users.Select(u => new TenantUser
        {
            TenantId = tenant.Id,
            UserId = u.Id
        }));
        await _dbContextMock.SaveChangesAsync();
        var query = new GridifyQuery { Page = 1, PageSize = 10, OrderBy = "email asc" };

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);

        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(false);

        // Act
        var pageOfUsers = await _userService.GetUsersAsync(query);

        // Assert
        pageOfUsers.Should().NotBeNull();
        var listOfUsers = pageOfUsers.Query.ToList();
        listOfUsers.Count.Should().Be(5);
        listOfUsers.Select(x => x.Email).Should().BeInAscendingOrder();
        pageOfUsers.Count.Should().Be(5);
    }

    [Fact]
    public async Task GetUsersAsync_DescendingOrderingAndPaginationWithFirstPage_CorrectNumberOfUsersInDescendingOrder()
    {
        // Arrange
        var users = TestData.TestUser.Generate(6);
        var tenant = TestData.TestTenant.Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Users.AddRangeAsync(users);
        await _dbContextMock.TenantUsers.AddRangeAsync(users.Select(u => new TenantUser
        {
            TenantId = tenant.Id,
            UserId = u.Id
        }));
        await _dbContextMock.SaveChangesAsync();
        var query = new GridifyQuery { Page = 1, PageSize = 4, OrderBy = "email desc" };

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);

        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(false);

        // Act
        var pageOfUsers = await _userService.GetUsersAsync(query);

        // Assert
        using (new AssertionScope())
        {
            pageOfUsers.Should().NotBeNull();
            var listOfUsers = pageOfUsers.Query.ToList();
            listOfUsers.Count.Should().Be(4);
            listOfUsers.Select(x => x.Email).Should().BeInDescendingOrder();
            pageOfUsers.Count.Should().Be(6);
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
        var pageOfUsers = await _userService.GetUsersAsync(query);

        // Assert
        pageOfUsers.Should().NotBeNull();
        pageOfUsers.Query.Should().BeEmpty();
        pageOfUsers.Count.Should().Be(0);
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

        _currentUserServiceMock
            .Setup(s => s.TenantRoles)
            .Returns(Array.Empty<string>());

        _currentUserServiceMock
            .Setup(s => s.Roles)
            .Returns(Array.Empty<string>());

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);

        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(false);

        //Act
        var actualResult = await _userService.UpdateUserStatusAsync(user.Id, Status.Active);

        //Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT0(out var updatedUser, out _).Should().BeTrue();
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            updatedUser.Status.Should().Be(Status.Active);
            updatedUser.DeactivationDateTimeUtc.Should().BeNull();
            updatedUser.ReactivationDateTimeUtc.Should().Be(currentDate);

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

        _currentUserServiceMock
            .Setup(s => s.TenantRoles)
            .Returns(Array.Empty<string>());

        _currentUserServiceMock
            .Setup(s => s.Roles)
            .Returns(Array.Empty<string>());

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);

        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(false);

        //Act
        var actualResult = await _userService.UpdateUserStatusAsync(user.Id, Status.Deactivated);

        //Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT0(out var updatedUser, out _).Should().BeTrue();
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            updatedUser.Status.Should().Be(Status.Deactivated);
            updatedUser.ReactivationDateTimeUtc.Should().BeNull();
            updatedUser.DeactivationDateTimeUtc.Should().Be(currentDate);

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Exactly(4));

            // TODO: Verify logs
        }
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
        actualResult.TryPickT0(out var actualUser, out _).Should().BeTrue();
        actualUser.Email.Should().Be(userEmail);
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

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);

        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(false);

        //Act
        _ = await _userService.ExportUsersAsync(fileType);

        //Assert
        switch (fileType)
        {
            case FileType.Csv:
                _csvMock.Verify(x => x.Convert(It.IsAny<List<TenantUserExportModel>>()), Times.Once());
                break;
            case FileType.Xlsx:
                _xlsxMock.Verify(x => x.Convert(It.IsAny<List<TenantUserExportModel>>()), Times.Once());
                break;
            default:
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

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);

        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(false);

        // Act
        var usersOneOf = await _userService.UpdateUserByIdAsync(Guid.NewGuid(), updateUserDto);

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
        var oldLegalName = user.LegalName;
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

        _currentUserServiceMock
            .Setup(s => s.TenantRoles)
            .Returns(Array.Empty<string>());

        _currentUserServiceMock
            .Setup(s => s.Roles)
            .Returns(Array.Empty<string>());

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);

        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(false);

        // Act
        var usersOneOf = await _userService.UpdateUserByIdAsync(user.Id, updateUserDto);

        // Assert
        using (new AssertionScope())
        {
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            usersOneOf.TryPickT0(out var userDto, out _);
            userDto.Should().NotBeNull();
            userDto.Id.Should().Be(user.Id);
            userDto.FirstName.Should().Be(updateUserDto.FirstName);
            userDto.FirstName.Should().NotBe(oldFirstName);
            userDto.LegalName.Should().Be(updateUserDto.LegalName);
            userDto.LegalName.Should().NotBe(oldLegalName);
            userDto.LastName.Should().Be(updateUserDto.LastName);
            userDto.LastName.Should().NotBe(oldLastName);

            var actualActivityLog = await _dbContextMock.UserActivityLogs.LastOrDefaultAsync();
            actualActivityLog.Should().NotBeNull();
            actualActivityLog?.Date.Should().Be(currentDate);
            actualActivityLog?.EventType.Should().Be(UserEventType.UserUpdated);
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
        _dateTimeServiceMock.SetupSequence(x => x.UtcNow)
            .Returns(DateTime.UtcNow)
            .Returns(DateTime.UtcNow);

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

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);

        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(false);

        //Act
        await _userService.ChangeUserRolesAsync(user.Id, roles.Select(x => x.Id).ToArray());

        //Assert
        _dbContextMock.TenantUserRoles.Count().Should().Be(2);
    }

    [Fact]
    public async Task AssignRoleAsync_ValidRoleIds_ShouldAssignAllProvidedRolesAndRemoveNotAssigned()
    {
        //Arrange
        _dateTimeServiceMock.SetupSequence(x => x.UtcNow)
            .Returns(DateTime.UtcNow)
            .Returns(DateTime.UtcNow);

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

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);

        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(false);

        //Act
        await _userService.ChangeUserRolesAsync(user.Id, new[] { roles.Select(x => x.Id).First() });

        //Assert
        _dbContextMock.TenantUserRoles.Count().Should().Be(1);
    }

    [Fact]
    public async Task GetActivitiesAsync_UserExists_ShouldCallAppropriateFactory()
    {
        //Arrange
        var tenant = TestData.TestTenant.Generate();
        tenant.Id = _tenantId;
        var user = TestData.TestUser.Generate();
        var tenantUser = new TenantUser
        {
            Tenant = tenant,
            User = user
        };
        var userActivity = new UserActivityLog
        {
            Date = DateTime.UtcNow,
            TenantId = tenant.Id,
            UserId = user.Id,
            EventType = UserEventType.UserCreated,
            Revision = 1
        };

        _dbContextMock.Tenants.Add(tenant);
        _dbContextMock.Users.Add(user);
        _dbContextMock.TenantUsers.Add(tenantUser);
        _dbContextMock.UserActivityLogs.Add(userActivity);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);

        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(false);

        //Act
        await _userService.GetActivitiesAsync(user.Id);

        //Assert

        _userCreatedActivityFactory.Verify(x => x.Create(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GetActivitiesAsync_UserDoesNotExistWithinTenant_ShouldReturnNotFount()
    {
        //Arrange
        var tenant = TestData.TestTenant.Generate();
        tenant.Id = _tenantId;
        var user = TestData.TestUser.Generate();

        _dbContextMock.Tenants.Add(tenant);
        _dbContextMock.Users.Add(user);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);

        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(false);

        //Act
        var resultOneOf = await _userService.GetActivitiesAsync(user.Id);

        //Assert
        resultOneOf.TryPickT1(out _, out _).Should().BeTrue();
    }

    [Fact]
    public async Task GetActivitiesAsync_UserExists_ShouldReturnExpectedNumberOfItems()
    {
        //Arrange
        var tenant = TestData.TestTenant.Generate();
        tenant.Id = _tenantId;
        var user = TestData.TestUser.Generate();
        var tenantUser = new TenantUser
        {
            Tenant = tenant,
            User = user
        };

        var activities = new[]
        {
            new UserActivityLog
            {
                Date = new DateTime(2000, 01, 1),
                TenantId = tenant.Id,
                UserId = user.Id,
                EventType = UserEventType.UserCreated,
                Revision = 1
            },
            new UserActivityLog
            {
                Date = new DateTime(2000, 01, 2),
                TenantId = tenant.Id,
                UserId = user.Id,
                EventType = UserEventType.UserCreated,
                Revision = 1
            },
            new UserActivityLog
            {
                Date = new DateTime(2000, 01, 3),
                TenantId = tenant.Id,
                UserId = user.Id,
                EventType = UserEventType.UserCreated,
                Revision = 1
            }
        };

        _dbContextMock.Tenants.Add(tenant);
        _dbContextMock.Users.Add(user);
        _dbContextMock.TenantUsers.Add(tenantUser);
        _dbContextMock.UserActivityLogs.AddRange(activities);
        await _dbContextMock.SaveChangesAsync();

        const int pageSize = 2;

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);

        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(false);

        //Act
        var resultOneOf = await _userService.GetActivitiesAsync(user.Id, 1, pageSize);

        //Assert
        using (new AssertionScope())
        {
            resultOneOf.TryPickT0(out var activitiesResult, out _).Should().BeTrue();
            activitiesResult.Count.Should().Be(2);
            activitiesResult.Query.Count().Should().Be(2);
        }
    }

    [Fact]
    public async Task GetUserPermissions_UserIsSuperAdmin_ReturnsNoTenantPermissions()
    {
        // Arrange
        var user = TestData.TestUser.Generate();
        var permissions = new Faker().Random
            .WordsArray(4)
            .Select(word => new Permission { Id = Guid.NewGuid(), Name = word })
            .ToList();
        var role = new Role { Id = Guid.NewGuid(), Name = new Faker().Random.Word() };

        await _dbContextMock.Users.AddAsync(user);
        await _dbContextMock.Roles.AddAsync(role);
        await _dbContextMock.Permissions.AddRangeAsync(permissions);
        await _dbContextMock.UserRoles.AddAsync(new UserRole { User = user, Role = role });
        await _dbContextMock.RolePermissions.AddRangeAsync(permissions
            .Select(permission => new RolePermission { Role = role, Permission = permission }));
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(Guid.Empty);

        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(true);

        // Act
        var actualResult = await _userService.GetUserPermissionsAsync(user.Id);

        // Assert
        actualResult.Should().BeEquivalentTo(permissions.Select(permission => permission.Name));
    }

    [Fact]
    public async Task GetUserPermissions_UserIdAndCurrentUserWithTenant_ReturnsTenantPermissions()
    {
        // Arrange
        var tenant = TestData.TestTenant.Generate();
        var user = TestData.TestUser.Generate();
        var permissions = new Faker().Random
            .WordsArray(4)
            .Select(word => new Permission
            {
                Id = Guid.NewGuid(),
                Name = word
            })
            .ToList();
        var role = new Role { Id = Guid.NewGuid(), Name = new Faker().Random.Word() };

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.TenantUsers.AddAsync(new TenantUser { Tenant = tenant, User = user });
        await _dbContextMock.TenantRoles.AddAsync(new TenantRole { Tenant = tenant, Role = role });
        await _dbContextMock.TenantPermissions.AddRangeAsync(permissions
            .Select(permission => new TenantPermission { Tenant = tenant, Permission = permission }));
        await _dbContextMock.TenantUserRoles.AddAsync(new TenantUserRole
        {
            TenantId = tenant.Id,
            UserId = user.Id,
            RoleId = role.Id
        });
        await _dbContextMock.TenantRolePermissions.AddRangeAsync(permissions
            .Select(permission => new TenantRolePermission
            {
                TenantId = tenant.Id,
                RoleId = role.Id,
                PermissionId = permission.Id
            }));
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _userService.GetUserPermissionsAsync(user.Id);

        // Assert
        actualResult.Should().BeEquivalentTo(permissions.Select(perm => perm.Name));
    }

    [Fact]
    public async Task GetUserInfoAsync_UserExists_ReturnsUserInfo()
    {
        // Arrange
        var tenant = TestData.TestTenant.Generate();
        var user = TestData.TestUser.Generate();
        var tenantRole = new Role { Id = Guid.NewGuid(), Name = new Faker().Random.Word() };
        var role = new Role { Id = Guid.NewGuid(), Name = new Faker().Random.Word() };

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Roles.AddRangeAsync(tenantRole, role);
        await _dbContextMock.TenantUsers.AddAsync(new TenantUser { Tenant = tenant, User = user });
        await _dbContextMock.TenantRoles.AddAsync(new TenantRole { Tenant = tenant, Role = tenantRole });
        await _dbContextMock.TenantUserRoles.AddAsync(new TenantUserRole
        {
            TenantId = tenant.Id,
            UserId = user.Id,
            RoleId = tenantRole.Id
        });
        await _dbContextMock.UserRoles.AddAsync(new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id
        });
        await _dbContextMock.SaveChangesAsync();

        // Act
        var actualResult = await _userService.GetUserInfoAsync(new MailAddress(user.Email), default);

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);

        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(false);

        // Assert
        using (new AssertionScope())
        {
            actualResult.Should().NotBeNull();
            actualResult!.FullName.Should().Be(user.FullName);
            actualResult!.Roles.Should().BeEquivalentTo(new[] { role.Name });
            actualResult!.TenantRoles.Should().BeEquivalentTo(new[] { tenantRole.Name });
            actualResult!.TenantId.Should().Be(tenant.Id);
        }
    }

    [Fact]
    public async Task GetUserInfoAsync_UserDoesNotExistsReturnsNull()
    {
        // Arrange
        var tenant = TestData.TestTenant.Generate();
        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);

        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(false);

        var email = "test@mail.xyz";

        // Act
        var actualResult = await _userService.GetUserInfoAsync(new MailAddress(email), default);

        // Assert
        actualResult.Should().BeNull();
    }

    [Fact]
    public async Task GetUserDetailsByIdAsync_UserRequestHisOwnData_ReturnsTenantAndNoTenantRolesInAscendingOrder()
    {
        // Arrange
        var tenant = TestData.TestTenant.Generate();
        var user = TestData.TestUser.Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Users.AddAsync(user);
        await _dbContextMock.TenantUsers.AddAsync(new TenantUser { Tenant = tenant, User = user });

        var roles = TestData.TestRoles.Generate(3).Select(r => r.Name).ToArray();
        var tenantRoles = TestData.TestRoles.Generate(3).Select(r => r.Name).ToArray();

        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(s => s.Roles)
            .Returns(roles);

        _currentUserServiceMock.Setup(s => s.TenantRoles)
            .Returns(tenantRoles);

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);

        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(false);

        _currentUserServiceMock
            .Setup(service => service.UserId)
            .Returns(user.Id);

        // Act
        var actualResult = await _userService.GetUserDetailsByIdAsync(user.Id);

        // Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT0(out var userDto, out _).Should().BeTrue();
            var rolesResult = userDto.Roles.Split(",").Select(r => r.Trim()).ToList();
            rolesResult.Should().BeInAscendingOrder();
            rolesResult.Should().BeEquivalentTo(roles.Concat(tenantRoles).Order());
        }
    }

    [Fact]
    public async Task CreateUserAsync_DivisionDoesNotExist_ReturnsInvalidOperation()
    {
        // Arrange
        var (_, departmentId, serviceAreaId, jobTitleId) = await SeedOrganizationUnits(_dbContextMock);
        var newUser = TestData.TestNewUser
            .CustomInstantiator(f => new CreateUserDto(
                f.Name.FirstName(),
                f.Name.FirstName(),
                f.Name.LastName(),
                f.Internet.Email(),
                Guid.NewGuid(),
                departmentId,
                serviceAreaId,
                jobTitleId,
                null))
            .Generate();

        _userExternalStore
            .Setup(x => x.CreateUserAsync(
                It.IsAny<MailAddress>(), It.IsAny<string>(), It.IsAny<string>(), default))
            .ReturnsAsync(string.Empty);

        // Act
        var result = await _userService.CreateUserAsync(newUser);

        // Assert
        using (new AssertionScope())
        {
            result.TryPickT2(out var error, out _).Should().BeTrue();
            error.Message.Should().Be($"Division with the ID {newUser.DivisionId} does not exist.");
        }
    }

    [Fact]
    public async Task CreateUserAsync_DepartmentDoesNotExist_ReturnsInvalidOperation()
    {
        // Arrange
        var (divisionId, _, serviceAreaId, jobTitleId) = await SeedOrganizationUnits(_dbContextMock);
        var newUser = TestData.TestNewUser
            .CustomInstantiator(f => new CreateUserDto(
                f.Name.FirstName(),
                f.Name.FirstName(),
                f.Name.LastName(),
                f.Internet.Email(),
                divisionId,
                Guid.NewGuid(),
                serviceAreaId,
                jobTitleId,
                null))
            .Generate();

        _userExternalStore
            .Setup(x => x.CreateUserAsync(
                It.IsAny<MailAddress>(), It.IsAny<string>(), It.IsAny<string>(), default))
            .ReturnsAsync(string.Empty);

        // Act
        var result = await _userService.CreateUserAsync(newUser);

        // Assert
        using (new AssertionScope())
        {
            result.TryPickT2(out var error, out _).Should().BeTrue();
            error.Message.Should().Be($"Department with the ID {newUser.DepartmentId} does not exist.");
        }
    }

    [Fact]
    public async Task CreateUserAsync_ServiceAreaDoesNotExist_ReturnsInvalidOperation()
    {
        // Arrange
        var (divisionId, departmentId, _, jobTitleId) = await SeedOrganizationUnits(_dbContextMock);
        var newUser = TestData.TestNewUser
            .CustomInstantiator(f => new CreateUserDto(
                f.Name.FirstName(),
                f.Name.FirstName(),
                f.Name.LastName(),
                f.Internet.Email(),
                divisionId,
                departmentId,
                Guid.NewGuid(),
                jobTitleId,
                null))
            .Generate();

        _userExternalStore
            .Setup(x => x.CreateUserAsync(
                It.IsAny<MailAddress>(), It.IsAny<string>(), It.IsAny<string>(), default))
            .ReturnsAsync(string.Empty);

        // Act
        var result = await _userService.CreateUserAsync(newUser);

        // Assert
        using (new AssertionScope())
        {
            result.TryPickT2(out var error, out _).Should().BeTrue();
            error.Message.Should().Be($"Service area with the ID {newUser.ServiceAreaId} does not exist.");
        }
    }

    [Fact]
    public async Task CreateUserAsync_JobTitleDoesNotExist_ReturnsInvalidOperation()
    {
        // Arrange
        var (divisionId, departmentId, serviceAreaId, _) = await SeedOrganizationUnits(_dbContextMock);
        var newUser = TestData.TestNewUser
            .CustomInstantiator(f => new CreateUserDto(
                f.Name.FirstName(),
                f.Name.FirstName(),
                f.Name.LastName(),
                f.Internet.Email(),
                divisionId,
                departmentId,
                serviceAreaId,
                Guid.NewGuid(),
                null))
            .Generate();

        _userExternalStore
            .Setup(x => x.CreateUserAsync(
                It.IsAny<MailAddress>(), It.IsAny<string>(), It.IsAny<string>(), default))
            .ReturnsAsync(string.Empty);

        // Act
        var result = await _userService.CreateUserAsync(newUser);

        // Assert
        using (new AssertionScope())
        {
            result.TryPickT2(out var error, out _).Should().BeTrue();
            error.Message.Should().Be($"Job title with the ID {newUser.JobTitleId} does not exist.");
        }
    }

    [Fact]
    public async Task CreateUserAsync_EmailAlreadyExists_ReturnsEmailAlreadyExists()
    {
        // Arrange
        var newUser = TestData.TestNewUser
            .CustomInstantiator(f => new CreateUserDto(
                f.Name.FirstName(),
                f.Name.FirstName(),
                f.Name.LastName(),
                f.Internet.Email(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid()))
            .Generate();
        var existingUser = TestData.TestUser.RuleFor(u => u.Email, newUser.Email);
        await _dbContextMock.Users.AddAsync(existingUser);
        await _dbContextMock.SaveChangesAsync();

        _userExternalStore
            .Setup(x => x.CreateUserAsync(
                It.IsAny<MailAddress>(), It.IsAny<string>(), It.IsAny<string>(), default))
            .ReturnsAsync(string.Empty);

        // Act
        var result = await _userService.CreateUserAsync(newUser);

        // Assert
        result.IsT1.Should().BeTrue();
    }

    [Fact]
    public async Task CreateUserAsync_ValidationPassesAndTeamIdIsNull_ReturnsNewUserAndCapturesActivityLog()
    {
        // Arrange
        var (divisionId, departmentId, serviceAreaId, jobTitleId) = await SeedOrganizationUnits(_dbContextMock);
        var newUser = TestData.TestNewUser
            .CustomInstantiator(f => new CreateUserDto(
                f.Name.FirstName(),
                f.Name.FirstName(),
                f.Name.LastName(),
                f.Internet.Email(),
                divisionId,
                departmentId,
                serviceAreaId,
                jobTitleId,
                null))
            .Generate();

        var currentDate = DateTime.UtcNow;
        _dateTimeServiceMock
            .Setup(service => service.UtcNow)
            .Returns(currentDate);

        _userExternalStore
            .Setup(x => x.CreateUserAsync(
                It.IsAny<MailAddress>(), It.IsAny<string>(), It.IsAny<string>(), default))
            .ReturnsAsync(string.Empty);

        // Act
        var result = await _userService.CreateUserAsync(newUser);

        // Assert
        using (new AssertionScope())
        {
            result.TryPickT0(out var userDto, out _).Should().BeTrue();
            userDto.Should().BeEquivalentTo(newUser,
                opt => opt.ExcludingMissingMembers());

            var activityLog = await _dbContextMock.UserActivityLogs.LastOrDefaultAsync();
            activityLog.Should().NotBeNull();
            activityLog!.Date.Should().Be(currentDate);
            activityLog.EventType.Should().Be(UserEventType.UserCreated);
            activityLog.TenantId.Should().Be(_tenantId);
            activityLog.UserId.Should().Be(userDto.Id);

            var userCreatedEvent = JsonSerializer.Deserialize<UserCreatedEvent>(activityLog.Event);
            userCreatedEvent.Should().NotBeNull();
            userCreatedEvent!.CreatedUserEmail.Should().Be(newUser.Email);

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Once);
        }
    }

    [Fact]
    public async Task CreateUserAsync_ValidationPassesAndTeamIdIsNotNull_ReturnsNewUserAndCapturesActivityLog()
    {
        // Arrange
        var (divisionId, departmentId, serviceAreaId, jobTitleId) = await SeedOrganizationUnits(_dbContextMock);
        var team = TestData.TestTeam
            .RuleFor(t => t.TenantId, _tenantId)
            .Generate();
        await _dbContextMock.Teams.AddAsync(team);
        await _dbContextMock.SaveChangesAsync();

        var newUser = TestData.TestNewUser
            .CustomInstantiator(f => new CreateUserDto(
                f.Name.FirstName(),
                f.Name.FirstName(),
                f.Name.LastName(),
                f.Internet.Email(),
                divisionId,
                departmentId,
                serviceAreaId,
                jobTitleId,
                team.Id))
            .Generate();

        var currentDate = DateTime.UtcNow;
        _dateTimeServiceMock
            .Setup(service => service.UtcNow)
            .Returns(currentDate);

        _userExternalStore
            .Setup(x => x.CreateUserAsync(
                It.IsAny<MailAddress>(), It.IsAny<string>(), It.IsAny<string>(), default))
            .ReturnsAsync(string.Empty);

        // Act
        var result = await _userService.CreateUserAsync(newUser);

        // Assert
        using (new AssertionScope())
        {
            result.TryPickT0(out var userDto, out _).Should().BeTrue();
            userDto.Should().BeEquivalentTo(newUser,
                opt => opt.ExcludingMissingMembers());

            var activityLog = await _dbContextMock.UserActivityLogs.LastOrDefaultAsync();
            activityLog.Should().NotBeNull();
            activityLog!.Date.Should().Be(currentDate);
            activityLog.EventType.Should().Be(UserEventType.UserCreated);
            activityLog.TenantId.Should().Be(_tenantId);
            activityLog.UserId.Should().Be(userDto.Id);

            var userCreatedEvent = JsonSerializer.Deserialize<UserCreatedEvent>(activityLog.Event);
            userCreatedEvent.Should().NotBeNull();
            userCreatedEvent!.CreatedUserEmail.Should().Be(newUser.Email);

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Once);
        }
    }

    private async Task<(Guid, Guid, Guid, Guid)> SeedOrganizationUnits(ITaxBeaconDbContext context)
    {
        var tenant = TestData.TestTenant
            .RuleFor(t => t.Id, _tenantId)
            .Generate();
        var division = TestData.TestDivision
            .RuleFor(d => d.TenantId, _tenantId)
            .Generate();
        var department = TestData.TestDepartment
            .RuleFor(d => d.DivisionId, division.Id)
            .RuleFor(d => d.TenantId, _tenantId)
            .Generate();
        var serviceArea = TestData.TestServiceArea
            .RuleFor(sa => sa.DepartmentId, department.Id)
            .RuleFor(sa => sa.TenantId, _tenantId)
            .Generate();
        var jobTitle = TestData.TestJobTitle
            .RuleFor(jt => jt.DepartmentId, department.Id)
            .RuleFor(jt => jt.TenantId, _tenantId)
            .Generate();

        await context.Tenants.AddAsync(tenant);
        await context.Divisions.AddAsync(division);
        await context.Departments.AddAsync(department);
        await context.ServiceAreas.AddAsync(serviceArea);
        await context.JobTitles.AddAsync(jobTitle);
        await context.SaveChangesAsync();

        return (division.Id, department.Id, serviceArea.Id, jobTitle.Id);
    }

    private static class TestData
    {
        public static readonly Faker<Tenant> TestTenant =
            new Faker<Tenant>()
                .RuleFor(t => t.Id, _ => Guid.NewGuid())
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.CreatedDateTimeUtc, _ => DateTime.UtcNow);

        public static readonly Faker<User> TestUser =
            new Faker<User>()
                .RuleFor(u => u.Id, _ => Guid.NewGuid())
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.LegalName, (_, u) => u.FirstName)
                .RuleFor(u => u.FullName, (_, u) => $"{u.FirstName} {u.LastName}")
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.CreatedDateTimeUtc, _ => DateTime.UtcNow)
                .RuleFor(u => u.Status, f => f.PickRandom<Status>());

        public static readonly Faker<UpdateUserDto> UpdateUserDtoFaker =
            new Faker<UpdateUserDto>()
                .RuleFor(dto => dto.FirstName, f => f.Name.FirstName())
                .RuleFor(dto => dto.LegalName, f => f.Name.FirstName())
                .RuleFor(dto => dto.LastName, f => f.Name.LastName());

        public static readonly Faker<Role> TestRoles =
            new Faker<Role>()
                .RuleFor(u => u.Id, _ => Guid.NewGuid())
                .RuleFor(u => u.Name, f => f.Name.JobTitle());

        public static readonly Faker<Division> TestDivision =
            new Faker<Division>()
                .RuleFor(d => d.Id, _ => Guid.NewGuid())
                .RuleFor(d => d.Name, f => f.Company.CompanyName());

        public static readonly Faker<Department> TestDepartment =
            new Faker<Department>()
                .RuleFor(d => d.Id, _ => Guid.NewGuid())
                .RuleFor(d => d.Name, f => f.Commerce.Department());

        public static readonly Faker<ServiceArea> TestServiceArea =
            new Faker<ServiceArea>()
                .RuleFor(sa => sa.Id, _ => Guid.NewGuid())
                .RuleFor(sa => sa.Name, f => f.Name.JobArea());

        public static readonly Faker<JobTitle> TestJobTitle =
            new Faker<JobTitle>()
                .RuleFor(jt => jt.Id, _ => Guid.NewGuid())
                .RuleFor(jt => jt.Name, f => f.Name.JobTitle());

        public static readonly Faker<Team> TestTeam =
            new Faker<Team>()
                .RuleFor(t => t.Id, _ => Guid.NewGuid())
                .RuleFor(t => t.Name, f => f.Company.CompanyName());

        public static readonly Faker<CreateUserDto> TestNewUser = new Faker<CreateUserDto>()
            .RuleFor(dto => dto.FirstName, f => f.Name.FirstName())
            .RuleFor(dto => dto.LegalName, f => f.Name.FirstName())
            .RuleFor(dto => dto.LastName, f => f.Name.LastName())
            .RuleFor(dto => dto.Email, f => f.Internet.Email());
    }
}
