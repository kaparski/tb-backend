using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OneOf.Types;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Security.Claims;
using TaxBeacon.Accounts.Accounts;
using TaxBeacon.Accounts.Accounts.Models;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Accounts;
using TaxBeacon.API.Controllers.Accounts.Requests;
using TaxBeacon.API.Controllers.Accounts.Responses;
using TaxBeacon.API.Controllers.Programs.Requests;
using TaxBeacon.API.Controllers.Programs.Responses;
using TaxBeacon.API.Exceptions;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.Common.Errors;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.API.UnitTests.Controllers.Accounts;

public sealed class AccountsControllerTests
{
    private readonly Mock<IAccountService> _accountServiceMock;
    private readonly AccountsController _controller;

    public AccountsControllerTests()
    {
        _accountServiceMock = new();
        _controller = new AccountsController(_accountServiceMock.Object)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new[]
                    {
                        new ClaimsIdentity(new[] { new Claim(Claims.Permission, "Accounts.Read") })
                    })
                }
            }
        };
    }

    [Fact]
    public void Get_ReturnSuccessStatusCode()
    {
        // Act
        var actualResponse = _controller.Get();

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<OkObjectResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeAssignableTo<IQueryable<AccountResponse>>();
        }
    }

    [Fact]
    public void Get_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<IActionResult>)_controller.Get).Method;

        // Act
        var hasPermissionsAttribute = methodInfo.GetCustomAttribute<HasPermissions>();

        // Assert
        using (new AssertionScope())
        {
            hasPermissionsAttribute.Should().NotBeNull();
            hasPermissionsAttribute?.Policy.Should().Be("Accounts.Read;Accounts.ReadWrite;Accounts.ReadExport");
        }
    }

    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public async Task ExportAccountsAsync_ValidQuery_ReturnsFileContent(FileType fileType)
    {
        // Arrange
        var request = new ExportAccountsRequest(fileType, "America/New_York");
        _accountServiceMock
            .Setup(x => x.ExportAccountsAsync(
                It.IsAny<FileType>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<byte>());

        // Act
        var actualResponse = await _controller.ExportAccountsAsync(request, default);

        // Assert
        using (new AssertionScope())
        {
            actualResponse.Should().NotBeNull();
            var actualResult = actualResponse as FileContentResult;
            actualResult.Should().NotBeNull();
            actualResult!.FileDownloadName.Should().Be($"accounts.{fileType.ToString().ToLowerInvariant()}");
            actualResult.ContentType.Should().Be(fileType switch
            {
                FileType.Csv => "text/csv",
                FileType.Xlsx => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => throw new InvalidOperationException()
            });
        }
    }

    [Fact]
    public void ExportAccountsAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo =
            ((Func<ExportAccountsRequest, CancellationToken, Task<IActionResult>>)_controller.ExportAccountsAsync)
            .Method;

        // Act
        var hasPermissionsAttribute = methodInfo.GetCustomAttribute<HasPermissions>();

        // Assert
        using (new AssertionScope())
        {
            hasPermissionsAttribute.Should().NotBeNull();
            hasPermissionsAttribute?.Policy.Should().Be("Accounts.ReadExport");
        }
    }

    [Fact]
    public async Task GetAccountDetailsAsync_AccountExists_ReturnsSuccessfulStatusCode()
    {
        // Arrange
        _accountServiceMock
            .Setup(x => x.GetAccountDetailsByIdAsync(It.IsAny<Guid>(), It.IsAny<AccountInfoType>(), default))
            .ReturnsAsync(new AccountDetailsDto());

        // Act
        var actualResponse = await _controller.GetAccountDetailsAsync(Guid.NewGuid(), default);

        // Assert
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeOfType<AccountDetailsResponse>();
        }
    }

    [Fact]
    public async Task GetAccountDetailsAsync_AccountDoesNotExist_ReturnsNotFoundStatusCode()
    {
        // Arrange
        _accountServiceMock
            .Setup(x => x.GetAccountDetailsByIdAsync(It.IsAny<Guid>(), It.IsAny<AccountInfoType>(), default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.GetAccountDetailsAsync(Guid.NewGuid(), default);

        // Assert
        using (new AssertionScope())
        {
            var actualResult = actualResponse as NotFoundResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }
    }

    [Fact]
    public async Task UpdateClientAsync_InvalidAccountId_ReturnsNotFoundResponse()
    {
        // Arrange
        var request = TestData.UpdateClientFaker.Generate();
        _accountServiceMock
            .Setup(service => service.UpdateClientDetailsAsync(
                It.IsAny<Guid>(),
                It.IsAny<UpdateClientDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.UpdateClientAsync(Guid.NewGuid(), request, default);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as NotFoundResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }
    }

    [Fact]
    public void UpdateClientAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo =
            ((Func<Guid, UpdateClientRequest, CancellationToken, Task<IActionResult>>)_controller.UpdateClientAsync)
            .Method;
        var permissions = new object[] { Common.Permissions.Clients.ReadWrite, Common.Permissions.Accounts.ReadWrite };

        // Act
        var hasPermissionsAttribute = methodInfo.GetCustomAttribute<HasPermissions>();

        // Assert
        using (new AssertionScope())
        {
            hasPermissionsAttribute.Should().NotBeNull();
            hasPermissionsAttribute?.Policy.Should()
                .Be(string.Join(";", permissions.Select(x => $"{x.GetType().Name}.{x}")));
        }
    }

    [Fact]
    public void GetAccountDetailsAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo =
            ((Func<Guid, CancellationToken, Task<IActionResult>>)_controller.GetAccountDetailsAsync)
            .Method;

        var permissions = new object[]
        {
            Common.Permissions.Accounts.Read, Common.Permissions.Accounts.ReadWrite,
            Common.Permissions.Accounts.ReadExport, Common.Permissions.Clients.Read,
            Common.Permissions.Clients.ReadWrite, Common.Permissions.Clients.ReadExport,
            Common.Permissions.Referrals.Read, Common.Permissions.Referrals.ReadWrite,
            Common.Permissions.Referrals.ReadExport,
        };

        // Act
        var hasPermissionsAttribute = methodInfo.GetCustomAttribute<HasPermissions>();

        // Assert
        using (new AssertionScope())
        {
            hasPermissionsAttribute.Should().NotBeNull();
            hasPermissionsAttribute?.Policy.Should()
                .Be(string.Join(";", permissions.Select(x => $"{x.GetType().Name}.{x}")));
        }
    }

    [Fact]
    public async Task UpdateAccountProfileAsync_AccountDoesNotExist_ReturnsNotFoundStatusCode()
    {
        // Arrange
        _accountServiceMock
            .Setup(x => x.UpdateAccountProfileAsync(It.IsAny<Guid>(), It.IsAny<UpdateAccountProfileDto>(), default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.UpdateAccountProfileAsync(Guid.NewGuid(),
            TestData.UpdateAccountProfileRequestFaker.Generate(),
            default);

        // Assert
        using (new AssertionScope())
        {
            var actualResult = actualResponse as NotFoundResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }
    }

    [Fact]
    public async Task UpdateAccountProfileAsync_AccountExist_ReturnsSuccessfulStatusCode()
    {
        // Arrange
        _accountServiceMock
            .Setup(x => x.UpdateAccountProfileAsync(It.IsAny<Guid>(), It.IsAny<UpdateAccountProfileDto>(), default))
            .ReturnsAsync(new AccountDetailsDto());

        // Act
        var actualResponse = await _controller.UpdateAccountProfileAsync(Guid.NewGuid(),
            TestData.UpdateAccountProfileRequestFaker.Generate(),
            default);

        // Assert
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeOfType<AccountDetailsResponse>();
        }
    }

    [Fact]
    public async Task UpdateAccountProfileAsync_InvalidOperation_ReturnsBadRequest()
    {
        // Arrange
        _accountServiceMock
            .Setup(x => x.UpdateAccountProfileAsync(It.IsAny<Guid>(), It.IsAny<UpdateAccountProfileDto>(), default))
            .ReturnsAsync(new InvalidOperation("error"));

        // Act
        var actualResponse = await _controller.UpdateAccountProfileAsync(Guid.NewGuid(),
            TestData.UpdateAccountProfileRequestFaker.Generate(),
            default);

        // Assert
        using (new AssertionScope())
        {
            var actualResult = actualResponse as BadRequestObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            actualResult?.Value.Should().BeOfType<InvalidOperationResponse>();
        }
    }

    [Fact]
    public async Task CreateAccountAsync_AccountDoesNotExists_ShouldReturnSuccessfulStatusCode()
    {
        // Arrange
        var request = TestData.CreateAccountRequestFaker.Generate();
        var account = TestData.AccountsDetailsDtoFaker.Generate();
        _accountServiceMock
            .Setup(x => x.CreateAccountAsync(It.IsAny<CreateAccountDto>(), AccountInfoType.Full, default))
            .ReturnsAsync(account);

        // Act
        var actualResponse = await _controller.CreateAccountAsync(request, default);

        // Assert
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeOfType<AccountDetailsResponse>();
        }
    }

    [Fact]
    public async Task CreateAccountAsync_InvalidOperation_ReturnsBadRequest()
    {
        // Arrange
        _accountServiceMock
            .Setup(x => x.CreateAccountAsync(It.IsAny<CreateAccountDto>(), AccountInfoType.Full, default))
            .ReturnsAsync(new InvalidOperation("error"));

        // Act
        var actualResponse = await _controller.CreateAccountAsync(TestData.CreateAccountRequestFaker.Generate(),
            default);

        // Assert
        using (new AssertionScope())
        {
            var actualResult = actualResponse as BadRequestObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            actualResult?.Value.Should().BeOfType<InvalidOperationResponse>();
        }
    }

    [Fact]
    public void CreateAccountAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo =
            ((Func<CreateAccountRequest, CancellationToken, Task<IActionResult>>)_controller.CreateAccountAsync)
            .Method;
        var permissions = new object[]
        {
            Common.Permissions.Clients.ReadWrite, Common.Permissions.Accounts.ReadWrite,
            Common.Permissions.Referrals.ReadWrite
        };

        // Act
        var hasPermissionsAttribute = methodInfo.GetCustomAttribute<HasPermissions>();

        // Assert
        using (new AssertionScope())
        {
            hasPermissionsAttribute.Should().NotBeNull();
            hasPermissionsAttribute?.Policy.Should()
                .Be(string.Join(";", permissions.Select(x => $"{x.GetType().Name}.{x}")));
        }
    }

    [Fact]
    public void GenerateAccountIdAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<CancellationToken, Task<IActionResult>>)
            _controller.GenerateUniqueAccountId).Method;
        var permissions = new object[] { Common.Permissions.Accounts.ReadWrite };

        // Act
        var hasPermissionsAttribute = methodInfo.GetCustomAttribute<HasPermissions>();

        // Assert
        using (new AssertionScope())
        {
            hasPermissionsAttribute.Should().NotBeNull();
            hasPermissionsAttribute?.Policy.Should()
                .Be(string.Join(";", permissions.Select(x => $"{x.GetType().Name}.{x}")));
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private static class TestData
    {
        public static readonly Faker<UpdateClientRequest> UpdateClientFaker =
            new Faker<UpdateClientRequest>()
                .RuleFor(a => a.AnnualRevenue, f => f.Finance.Amount())
                .RuleFor(a => a.EmployeeCount, f => f.Random.Number(0, 1000))
                .RuleFor(a => a.FoundationYear, f => f.Random.Number(1900, 2023))
                .RuleFor(dto => dto.ClientManagersIds, f => f.Make(3, Guid.NewGuid));

        public static readonly Faker<AccountDetailsDto> AccountsDetailsDtoFaker =
            new Faker<AccountDetailsDto>()
                .RuleFor(a => a.Id, _ => Guid.NewGuid())
                .RuleFor(a => a.Name, f => f.Company.CompanyName())
                .RuleFor(a => a.Website, f => f.Internet.Url())
                .RuleFor(a => a.Country, f => f.Address.Country())
                .RuleFor(a => a.State, f => f.PickRandom<State>())
                .RuleFor(a => a.City, f => f.Address.City())
                .RuleFor(a => a.Address1, f => f.Address.StreetAddress())
                .RuleFor(a => a.Address2, f => f.Address.StreetAddress())
                .RuleFor(a => a.Zip, f => f.Random.Number(10000, 9999999).ToString())
                .RuleFor(a => a.County, f => f.Address.County());

        public static readonly Faker<CreateAccountRequest> CreateAccountRequestFaker =
            new Faker<CreateAccountRequest>()
                .RuleFor(a => a.Name, f => f.Company.CompanyName())
                .RuleFor(a => a.Website, f => f.Internet.Url())
                .RuleFor(a => a.Country, _ => Country.UnitedStates)
                .RuleFor(a => a.State, f => f.PickRandom<State>())
                .RuleFor(a => a.City, f => f.Address.City())
                .RuleFor(a => a.Address1, f => f.Address.StreetAddress())
                .RuleFor(a => a.Address2, f => f.Address.StreetAddress())
                .RuleFor(a => a.Zip, f => f.Random.Number(10000, 9999999).ToString())
                .RuleFor(a => a.County, f => f.Address.County());

        public static readonly Faker<UpdateAccountProfileRequest> UpdateAccountProfileRequestFaker =
            new Faker<UpdateAccountProfileRequest>()
                .RuleFor(a => a.Name, f => f.Company.CompanyName())
                .RuleFor(a => a.Website, f => f.Internet.Url())
                .RuleFor(a => a.Country, _ => Country.UnitedStates)
                .RuleFor(a => a.State, f => f.PickRandom<State>())
                .RuleFor(a => a.City, f => f.Address.City())
                .RuleFor(a => a.Address1, f => f.Address.StreetAddress())
                .RuleFor(a => a.Address2, f => f.Address.StreetAddress())
                .RuleFor(a => a.Zip, f => f.Random.Number(10000, 9999999).ToString())
                .RuleFor(a => a.County, f => f.Address.County());
    }
}
