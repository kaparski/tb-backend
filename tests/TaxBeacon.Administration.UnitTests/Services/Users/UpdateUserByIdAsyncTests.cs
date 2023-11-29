using FluentAssertions;
using FluentAssertions.Execution;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.Administration.UnitTests.Services.Users;

public partial class UserServiceTests
{
    [Fact]
    public async Task UpdateUserByIdAsync_InvalidUserId_ReturnsNotFound()
    {
        // Arrange
        var updateUserDto = TestData.UpdateUserDtoFaker.Generate();
        var user = TestData.UserFaker.Generate();
        var tenant = TestData.TenantFaker.Generate();
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
        TypeAdapterConfig<TenantUserView, User>
            .ForType()
            .Ignore(dest => dest.Division)
            .Ignore(dest => dest.Department)
            .Ignore(dest => dest.ServiceArea)
            .Ignore(dest => dest.JobTitle)
            .Ignore(dest => dest.Team);

        var (divisionId, departmentId, serviceAreaId, jobTitleId, tenantId) =
            await TestData.SeedOrganizationUnits(_dbContextMock);
        var updateUserDto = TestData.UpdateUserDtoFaker.Generate();
        updateUserDto.DivisionId = divisionId;
        updateUserDto.DepartmentId = departmentId;
        updateUserDto.ServiceAreaId = serviceAreaId;
        updateUserDto.JobTitleId = jobTitleId;
        var userView = TestData.TenantUserViewFaker.Generate();
        userView.TenantId = tenantId;
        var user = userView.Adapt<User>();
        updateUserDto.Adapt(userView);
        var oldFirstName = user.FirstName;
        var oldLastName = user.LastName;
        var oldLegalName = user.LegalName;
        var currentDate = DateTime.UtcNow;

        await _dbContextMock.Users.AddAsync(user);
        await _dbContextMock.TenantUsersView.AddAsync(userView);
        await _dbContextMock.TenantUsers.AddAsync(new TenantUser { TenantId = tenantId, User = user });

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
            .Returns(tenantId);

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

            var updatedUser = await _dbContextMock.Users.SingleOrDefaultAsync(u => u.Id == user.Id);
            updatedUser.Should().NotBeNull();
            updatedUser?.FirstName.Should().Be(updateUserDto.FirstName);
            updatedUser?.FirstName.Should().NotBe(oldFirstName);
            updatedUser?.LegalName.Should().Be(updateUserDto.LegalName);
            updatedUser?.LegalName.Should().NotBe(oldLegalName);
            updatedUser?.LastName.Should().Be(updateUserDto.LastName);
            updatedUser?.LastName.Should().NotBe(oldLastName);

            var actualActivityLog = await _dbContextMock.UserActivityLogs.LastOrDefaultAsync();
            actualActivityLog.Should().NotBeNull();
            actualActivityLog?.Date.Should().Be(currentDate);
            actualActivityLog?.EventType.Should().Be(UserEventType.UserUpdated);
            actualActivityLog?.UserId.Should().Be(user.Id);
            actualActivityLog?.TenantId.Should().Be(tenantId);

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Once);
        }
    }

    [Fact]
    public async Task UpdateUserByIdAsync_InvalidDivisionIdAndDivisionsEnabled_ReturnsInvalidOperation()
    {
        // Arrange
        var updateUserDto = TestData.UpdateUserDtoFaker.Generate();
        updateUserDto.DivisionId = Guid.NewGuid();
        var user = TestData.UserFaker.Generate();
        var tenant = TestData.TenantFaker.Generate();
        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Users.AddAsync(user);
        await _dbContextMock.TenantUsers.AddAsync(new TenantUser { TenantId = tenant.Id, User = user });
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);

        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(false);

        _currentUserServiceMock
            .Setup(service => service.DivisionEnabled)
            .Returns(true);

        // Act
        var usersOneOf = await _userService.UpdateUserByIdAsync(user.Id, updateUserDto);

        // Assert
        using (new AssertionScope())
        {
            usersOneOf.TryPickT2(out var error, out _).Should().BeTrue();
            error?.Message.Should().Be($"Division with the ID {updateUserDto.DivisionId} does not exist.");
        }
    }

    [Fact]
    public async Task UpdateUserByIdAsync_InvalidDepartmentIdAndDivisionsEnabled_ReturnsInvalidOperation()
    {
        // Arrange
        var (divisionId, _, serviceAreaId, jobTitleId, tenantId) = await TestData.SeedOrganizationUnits(_dbContextMock);
        var updateUserDto = TestData.UpdateUserDtoFaker.Generate();
        updateUserDto.DivisionId = divisionId;
        updateUserDto.DepartmentId = Guid.NewGuid();
        updateUserDto.ServiceAreaId = serviceAreaId;
        updateUserDto.JobTitleId = jobTitleId;
        var user = TestData.UserFaker.Generate();
        await _dbContextMock.Users.AddAsync(user);
        await _dbContextMock.TenantUsers.AddAsync(new TenantUser { TenantId = tenantId, User = user });
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenantId);

        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(false);

        _currentUserServiceMock
            .Setup(service => service.DivisionEnabled)
            .Returns(true);

        // Act
        var usersOneOf = await _userService.UpdateUserByIdAsync(user.Id, updateUserDto);

        // Assert
        using (new AssertionScope())
        {
            usersOneOf.TryPickT2(out var error, out _).Should().BeTrue();
            error?.Message.Should().Be($"Department with the ID {updateUserDto.DepartmentId} does not exist.");
        }
    }

    [Fact]
    public async Task UpdateUserByIdAsync_InvalidServiceAreaIdAndDivisionsEnabled_ReturnsInvalidOperation()
    {
        // Arrange
        var (divisionId, departmentId, _, jobTitleId, tenantId) = await TestData.SeedOrganizationUnits(_dbContextMock);
        var updateUserDto = TestData.UpdateUserDtoFaker.Generate();
        updateUserDto.DivisionId = divisionId;
        updateUserDto.DepartmentId = departmentId;
        updateUserDto.ServiceAreaId = Guid.NewGuid();
        updateUserDto.JobTitleId = jobTitleId;
        var user = TestData.UserFaker.Generate();
        await _dbContextMock.Users.AddAsync(user);
        await _dbContextMock.TenantUsers.AddAsync(new TenantUser { TenantId = tenantId, User = user });
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenantId);

        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(false);

        _currentUserServiceMock
            .Setup(service => service.DivisionEnabled)
            .Returns(true);

        // Act
        var usersOneOf = await _userService.UpdateUserByIdAsync(user.Id, updateUserDto);

        // Assert
        using (new AssertionScope())
        {
            usersOneOf.TryPickT2(out var error, out _).Should().BeTrue();
            error?.Message.Should().Be($"Service area with the ID {updateUserDto.ServiceAreaId} does not exist.");
        }
    }

    [Fact]
    public async Task UpdateUserByIdAsync_InvalidJobTitleIdAndDivisionsEnabled_ReturnsInvalidOperation()
    {
        // Arrange
        var (divisionId, departmentId, serviceAreaId, _, tenantId) =
            await TestData.SeedOrganizationUnits(_dbContextMock);
        var updateUserDto = TestData.UpdateUserDtoFaker.Generate();
        updateUserDto.DivisionId = divisionId;
        updateUserDto.DepartmentId = departmentId;
        updateUserDto.ServiceAreaId = serviceAreaId;
        updateUserDto.JobTitleId = Guid.NewGuid();
        var user = TestData.UserFaker.Generate();
        await _dbContextMock.Users.AddAsync(user);
        await _dbContextMock.TenantUsers.AddAsync(new TenantUser { TenantId = tenantId, User = user });
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenantId);

        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(false);

        _currentUserServiceMock
            .Setup(service => service.DivisionEnabled)
            .Returns(true);

        // Act
        var usersOneOf = await _userService.UpdateUserByIdAsync(user.Id, updateUserDto);

        // Assert
        using (new AssertionScope())
        {
            usersOneOf.TryPickT2(out var error, out _).Should().BeTrue();
            error?.Message.Should().Be($"Job title with the ID {updateUserDto.JobTitleId} does not exist.");
        }
    }

    [Fact]
    public async Task UpdateUserByIdAsync_InvalidTeamIdAndDivisionsEnabled_ReturnsInvalidOperation()
    {
        // Arrange
        var updateUserDto = TestData.UpdateUserDtoFaker.Generate();
        updateUserDto.TeamId = Guid.NewGuid();
        var user = TestData.UserFaker.Generate();
        var tenant = TestData.TenantFaker.Generate();
        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Users.AddAsync(user);
        await _dbContextMock.TenantUsers.AddAsync(new TenantUser { TenantId = tenant.Id, User = user });
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);

        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(false);

        _currentUserServiceMock
            .Setup(service => service.DivisionEnabled)
            .Returns(true);

        // Act
        var usersOneOf = await _userService.UpdateUserByIdAsync(user.Id, updateUserDto);

        // Assert
        using (new AssertionScope())
        {
            usersOneOf.TryPickT2(out var error, out _).Should().BeTrue();
            error?.Message.Should().Be($"Team with the ID {updateUserDto.TeamId} does not exist.");
        }
    }

    [Fact]
    public async Task UpdateUserByIdAsync_DivisionAndDepartmentDontMatchAndDivisionsEnabled_ReturnsInvalidOperation()
    {
        // Arrange
        var updateUserDto = TestData.UpdateUserDtoFaker.Generate();
        var user = TestData.UserFaker.Generate();
        var tenant = TestData.TenantFaker.Generate();
        var divisions = TestData.DivisionFaker.RuleFor(d => d.TenantId, _ => tenant.Id).Generate(3);
        var department = TestData.DepartmentFaker
            .RuleFor(d => d.DivisionId, _ => divisions[1].Id)
            .RuleFor(d => d.TenantId, _ => tenant.Id)
            .Generate();
        updateUserDto.DivisionId = divisions[0].Id;
        updateUserDto.DepartmentId = department.Id;
        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Users.AddAsync(user);
        await _dbContextMock.TenantUsers.AddAsync(new TenantUser { TenantId = tenant.Id, User = user });
        await _dbContextMock.Divisions.AddRangeAsync(divisions);
        await _dbContextMock.Departments.AddAsync(department);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);

        _currentUserServiceMock
            .Setup(service => service.IsSuperAdmin)
            .Returns(false);

        _currentUserServiceMock
            .Setup(service => service.DivisionEnabled)
            .Returns(true);

        // Act
        var usersOneOf = await _userService.UpdateUserByIdAsync(user.Id, updateUserDto);

        // Assert
        using (new AssertionScope())
        {
            usersOneOf.IsT0.Should().BeFalse();
            usersOneOf.IsT1.Should().BeFalse();
            usersOneOf.TryPickT2(out var error, out _).Should().BeTrue();
            error?.ParamName.Should().Be("divisionAndDepartment");
            error?.Message.Should()
                .Be($"Division {updateUserDto.DivisionId} and Department {updateUserDto.DepartmentId} do not match.");
        }
    }
}
