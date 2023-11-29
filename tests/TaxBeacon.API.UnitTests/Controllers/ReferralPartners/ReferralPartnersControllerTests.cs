using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Reflection;
using TaxBeacon.Accounts.Accounts;
using TaxBeacon.Accounts.Accounts.Models;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.ReferralPartners;
using TaxBeacon.API.Controllers.ReferralPartners.Requests;
using TaxBeacon.API.Controllers.ReferralPartners.Responses;
using TaxBeacon.API.Controllers.ReferralProspects.Requests;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.UnitTests.Controllers.ReferralPartners;
public class ReferralPartnersControllerTests
{
    private readonly Mock<IAccountService> _accountServiceMock;
    private readonly ReferralPartnersController _controller;

    public ReferralPartnersControllerTests()
    {
        _accountServiceMock = new();
        _controller = new ReferralPartnersController(_accountServiceMock.Object);
    }

    [Fact]
    public void GetReferralPartners_HasSuccessStatusCode()
    {
        // Arrange
        _accountServiceMock
            .Setup(p => p.QueryReferralPartners())
            .Returns(Enumerable.Empty<ReferralPartnerDto>().AsQueryable());

        // Act
        var actualResponse = _controller.GetReferralPartners();

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<OkObjectResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeAssignableTo<IQueryable<ReferralPartnerResponse>>();
        }
    }

    [Fact]
    public void GetReferralPartners_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<IActionResult>)_controller.GetReferralPartners).Method;

        // Act
        var hasPermissionsAttribute = methodInfo.GetCustomAttribute<HasPermissions>();
        var permissions = new object[]
        {
            Common.Permissions.Referrals.Read,
            Common.Permissions.Accounts.Read,
        };

        // Assert
        using (new AssertionScope())
        {
            hasPermissionsAttribute.Should().NotBeNull();
            hasPermissionsAttribute?.Policy.Should()
                .Be(string.Join(";", permissions.Select(x => $"{x.GetType().Name}.{x}")));
        }
    }

    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public async Task ExportReferralPartnersAsync_ValidQuery_ReturnsFileContent(FileType fileType)
    {
        // Arrange
        var request = new ExportReferralPartnersRequest(fileType, "America/New_York");
        _accountServiceMock
            .Setup(x => x.ExportReferralProspectsAsync(
                It.IsAny<FileType>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<byte>());

        // Act
        var actualResponse = await _controller.ExportReferralPartnersAsync(request, default);

        // Assert
        using (new AssertionScope())
        {
            actualResponse.Should().NotBeNull();
            var actualResult = actualResponse as FileContentResult;
            actualResult.Should().NotBeNull();
            actualResult!.FileDownloadName.Should().Be($"referral-partners.{fileType.ToString().ToLowerInvariant()}");
            actualResult.ContentType.Should().Be(fileType switch
            {
                FileType.Csv => "text/csv",
                FileType.Xlsx => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => throw new InvalidOperationException()
            });
        }
    }

    [Fact]
    public void ExportReferralPartnersAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo =
            ((Func<ExportReferralPartnersRequest, CancellationToken, Task<IActionResult>>)_controller.ExportReferralPartnersAsync)
            .Method;

        // Act
        var hasPermissionsAttribute = methodInfo.GetCustomAttribute<HasPermissions>();

        var permissions = new object[]
        {
            Common.Permissions.Referrals.ReadExport,
            Common.Permissions.Accounts.ReadExport,
        };

        // Assert
        using (new AssertionScope())
        {
            hasPermissionsAttribute.Should().NotBeNull();
            hasPermissionsAttribute?.Policy.Should().Be(string.Join(";", permissions.Select(x => $"{x.GetType().Name}.{x}")));
        }
    }
}
