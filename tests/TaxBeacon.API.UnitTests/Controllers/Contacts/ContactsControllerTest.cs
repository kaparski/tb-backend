using FluentAssertions;
using FluentAssertions.Execution;
using Gridify;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
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
    public void Get_ValidQuery_ReturnSuccessStatusCode()
    {
        // Arrange
        _contactServiceMock.Setup(p => p.QueryContacts(It.IsAny<Guid>())).Returns(
                Enumerable.Empty<ContactDto>().AsQueryable());

        // Act
        var actualResponse = _controller.Get(new Guid());

        // Arrange
        actualResponse.Should().NotBeNull();
        actualResponse?.Should().BeAssignableTo<IQueryable<ContactResponse>>();
    }
}
