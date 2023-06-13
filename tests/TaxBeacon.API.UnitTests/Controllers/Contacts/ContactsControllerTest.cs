using FluentAssertions;
using FluentAssertions.Execution;
using Gridify;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OneOf;
using OneOf.Types;
using TaxBeacon.Accounts.Services.Contacts;
using TaxBeacon.Accounts.Services.Contacts.Models;
using TaxBeacon.API.Controllers.Contacts;
using TaxBeacon.API.Controllers.Contacts.Responses;
using TaxBeacon.API.Controllers.Users.Responses;

namespace TaxBeacon.API.UnitTests.Controllers.Contacts;

public class ContactsControllerTest
{
    private readonly Mock<IContactService> _contactServiceMock;
    private readonly ContactsController _controller;

    public ContactsControllerTest()
    {
        _contactServiceMock = new();
        _controller = new ContactsController(_contactServiceMock.Object);
    }

    [Fact]
    public async Task Get_ExistingAccountId_ReturnSuccessStatusCode()
    {
        // Arrange
        _contactServiceMock.Setup(p => p.QueryContactsAsync(It.IsAny<Guid>())).ReturnsAsync(
                new Success<IQueryable<ContactDto>>(Enumerable.Empty<ContactDto>().AsQueryable()));

        // Act
        var actualResponse = await _controller.Get(new Guid());

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<OkObjectResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeAssignableTo<IQueryable<ContactResponse>>();
        }
    }

    [Fact]
    public async Task Get_NonExistingAccountId_ReturnNotFoundStatusCode()
    {
        // Arrange
        _contactServiceMock.Setup(p => p.QueryContactsAsync(It.IsAny<Guid>())).ReturnsAsync(
            new NotFound());

        // Act
        var actualResponse = await _controller.Get(new Guid());

        // Arrange
        // Assert
        using (new AssertionScope())
        {
            var actualResult = actualResponse as NotFoundResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }
    }
}
