using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Net.Mail;
using System.Text.Json;
using TaxBeacon.Administration.Users.Activities.Models;
using TaxBeacon.Administration.Users.Models;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.Administration.UnitTests.Services.Users;

public partial class UserServiceTests
{
    [Fact]
    public async Task CreateUserAsync_DivisionDoesNotExist_ReturnsInvalidOperation()
    {
        // Arrange
        var (_, departmentId, serviceAreaId, jobTitleId, tenantId) =
            await TestData.SeedOrganizationUnits(_dbContextMock);
        var newUser = TestData.CreateUserDtoFaker
            .CustomInstantiator(f => new CreateUserDto(
                f.Name.FirstName(),
                f.Name.FirstName(),
                f.Name.LastName(),
                f.Internet.Email(),
                string.Empty,
                Guid.NewGuid(),
                departmentId,
                serviceAreaId,
                jobTitleId,
                null))
            .Generate();

        _userExternalStore
            .Setup(x => x.CreateUserAsync(
                It.IsAny<MailAddress>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                default))
            .ReturnsAsync((string.Empty, UserType.ExistingB2C, string.Empty));

        _currentUserServiceMock.Setup(s => s.TenantId).Returns(tenantId);

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
        var (divisionId, _, serviceAreaId, jobTitleId, tenantId) = await TestData.SeedOrganizationUnits(_dbContextMock);
        var newUser = TestData.CreateUserDtoFaker
            .CustomInstantiator(f => new CreateUserDto(
                f.Name.FirstName(),
                f.Name.FirstName(),
                f.Name.LastName(),
                f.Internet.Email(),
                string.Empty,
                divisionId,
                Guid.NewGuid(),
                serviceAreaId,
                jobTitleId,
                null))
            .Generate();

        _userExternalStore
            .Setup(x => x.CreateUserAsync(
                It.IsAny<MailAddress>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                default))
            .ReturnsAsync((string.Empty, UserType.ExistingB2C, string.Empty));

        _currentUserServiceMock.Setup(s => s.TenantId).Returns(tenantId);

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
        var (divisionId, departmentId, _, jobTitleId, tenantId) = await TestData.SeedOrganizationUnits(_dbContextMock);
        var newUser = TestData.CreateUserDtoFaker
            .CustomInstantiator(f => new CreateUserDto(
                f.Name.FirstName(),
                f.Name.FirstName(),
                f.Name.LastName(),
                f.Internet.Email(),
                string.Empty,
                divisionId,
                departmentId,
                Guid.NewGuid(),
                jobTitleId,
                null))
            .Generate();

        _userExternalStore
            .Setup(x => x.CreateUserAsync(
                It.IsAny<MailAddress>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                default))
            .ReturnsAsync((string.Empty, UserType.ExistingB2C, string.Empty));

        _currentUserServiceMock.Setup(s => s.TenantId).Returns(tenantId);

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
        var (divisionId, departmentId, serviceAreaId, _, tenantId) =
            await TestData.SeedOrganizationUnits(_dbContextMock);
        var newUser = TestData.CreateUserDtoFaker
            .CustomInstantiator(f => new CreateUserDto(
                f.Name.FirstName(),
                f.Name.FirstName(),
                f.Name.LastName(),
                f.Internet.Email(),
                string.Empty,
                divisionId,
                departmentId,
                serviceAreaId,
                Guid.NewGuid(),
                null))
            .Generate();

        _userExternalStore
            .Setup(x => x.CreateUserAsync(
                It.IsAny<MailAddress>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                default))
            .ReturnsAsync((string.Empty, UserType.ExistingB2C, string.Empty));

        _currentUserServiceMock.Setup(s => s.TenantId).Returns(tenantId);

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
        var newUser = TestData.CreateUserDtoFaker
            .CustomInstantiator(f => new CreateUserDto(
                f.Name.FirstName(),
                f.Name.FirstName(),
                f.Name.LastName(),
                f.Internet.Email(),
                string.Empty,
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid()))
            .Generate();
        var existingUser = TestData.UserFaker.RuleFor(u => u.Email, newUser.Email);
        await _dbContextMock.Users.AddAsync(existingUser);

        await _dbContextMock.TenantUsers.AddAsync(new TenantUser { TenantId = _tenantId, User = existingUser });

        await _dbContextMock.SaveChangesAsync();

        _userExternalStore
            .Setup(x => x.CreateUserAsync(
                It.IsAny<MailAddress>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                default))
            .ReturnsAsync((string.Empty, UserType.ExistingB2C, string.Empty));

        // Act
        var result = await _userService.CreateUserAsync(newUser);

        // Assert
        result.IsT1.Should().BeTrue();
    }

    [Fact]
    public async Task CreateUserAsync_ValidationPassesAndTeamIdIsNull_ReturnsNewUserAndCapturesActivityLog()
    {
        // Arrange
        var (divisionId, departmentId, serviceAreaId, jobTitleId, tenantId) =
            await TestData.SeedOrganizationUnits(_dbContextMock);
        var newUser = TestData.CreateUserDtoFaker
            .CustomInstantiator(f => new CreateUserDto(
                f.Name.FirstName(),
                f.Name.FirstName(),
                f.Name.LastName(),
                f.Internet.Email(),
                string.Empty,
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
                It.IsAny<MailAddress>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                default))
            .ReturnsAsync((string.Empty, UserType.ExistingB2C, string.Empty));

        _currentUserServiceMock.Setup(s => s.TenantId).Returns(tenantId);

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
            activityLog.TenantId.Should().Be(tenantId);
            activityLog.UserId.Should().Be(userDto.Id);

            var userCreatedEvent = JsonSerializer.Deserialize<UserCreatedEvent>(activityLog.Event);
            userCreatedEvent.Should().NotBeNull();
            userCreatedEvent!.CreatedUserEmail.Should().Be(newUser.Email);

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Exactly(2));
        }
    }

    [Fact]
    public async Task CreateUserAsync_ValidationPassesAndTeamIdIsNotNull_ReturnsNewUserAndCapturesActivityLog()
    {
        // Arrange
        var (divisionId, departmentId, serviceAreaId, jobTitleId, tenantId) =
            await TestData.SeedOrganizationUnits(_dbContextMock);
        var team = TestData.TeamFaker
            .RuleFor(t => t.TenantId, tenantId)
            .Generate();
        await _dbContextMock.Teams.AddAsync(team);
        await _dbContextMock.SaveChangesAsync();

        var newUser = TestData.CreateUserDtoFaker
            .CustomInstantiator(f => new CreateUserDto(
                f.Name.FirstName(),
                f.Name.FirstName(),
                f.Name.LastName(),
                f.Internet.Email(),
                null,
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
                It.IsAny<MailAddress>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                default))
            .ReturnsAsync((string.Empty, UserType.ExistingB2C, string.Empty));

        _currentUserServiceMock.Setup(s => s.TenantId).Returns(tenantId);

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
            activityLog.TenantId.Should().Be(tenantId);
            activityLog.UserId.Should().Be(userDto.Id);

            var userCreatedEvent = JsonSerializer.Deserialize<UserCreatedEvent>(activityLog.Event);
            userCreatedEvent.Should().NotBeNull();
            userCreatedEvent!.CreatedUserEmail.Should().Be(newUser.Email);

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Exactly(2));
        }
    }

    [Fact]
    public async Task CreateUserAsync_ValidationPasses_ReturnsNewUserAndCapturesActivityCredentialSentActivityLog()
    {
        // Arrange
        var (divisionId, departmentId, serviceAreaId, jobTitleId, tenantId) =
            await TestData.SeedOrganizationUnits(_dbContextMock);
        var team = TestData.TeamFaker
            .RuleFor(t => t.TenantId, tenantId)
            .Generate();
        await _dbContextMock.Teams.AddAsync(team);
        await _dbContextMock.SaveChangesAsync();

        var newUser = TestData.CreateUserDtoFaker
            .CustomInstantiator(f => new CreateUserDto(
                f.Name.FirstName(),
                f.Name.FirstName(),
                f.Name.LastName(),
                f.Internet.Email(),
                null,
                divisionId,
                departmentId,
                serviceAreaId,
                jobTitleId,
                team.Id))
            .Generate();

        var currentDate = DateTime.UtcNow;
        var currentDate2 = currentDate.AddSeconds(1);
        _dateTimeServiceMock
            .SetupSequence(service => service.UtcNow)
            .Returns(currentDate)
            .Returns(currentDate2);

        _userExternalStore
            .Setup(x => x.CreateUserAsync(
                It.IsAny<MailAddress>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                default))
            .ReturnsAsync((string.Empty, UserType.LocalB2C, string.Empty));

        _currentUserServiceMock.Setup(s => s.TenantId).Returns(tenantId);

        // Act
        var result = await _userService.CreateUserAsync(newUser);

        // Assert
        using (new AssertionScope())
        {
            result.TryPickT0(out var userDto, out _).Should().BeTrue();
            userDto.Should().BeEquivalentTo(newUser,
                opt => opt.ExcludingMissingMembers());

            _dbContextMock.UserActivityLogs.Count().Should().Be(2);

            var credentialsActivityLog = await _dbContextMock.UserActivityLogs.LastOrDefaultAsync();
            credentialsActivityLog.Should().NotBeNull();
            credentialsActivityLog!.Date.Should().Be(currentDate);
            credentialsActivityLog.EventType.Should().Be(UserEventType.CredentialSent);
            credentialsActivityLog.TenantId.Should().Be(tenantId);
            credentialsActivityLog.UserId.Should().Be(userDto.Id);

            var credentialsSentEvent = JsonSerializer.Deserialize<CredentialsSentEvent>(credentialsActivityLog.Event);
            credentialsSentEvent.Should().NotBeNull();
            credentialsSentEvent!.CreatedUserEmail.Should().Be(newUser.Email);

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Exactly(2));
        }
    }
}
