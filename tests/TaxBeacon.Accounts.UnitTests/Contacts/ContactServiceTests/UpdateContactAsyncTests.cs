// ReSharper disable once CheckNamespace

using FluentAssertions;
using FluentAssertions.Execution;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaxBeacon.Accounts.Contacts.Models;
using TaxBeacon.Common.Enums.Accounts.Activities;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.Accounts.UnitTests.Contacts;

public partial class ContactServiceTests
{

    [Fact]
    public async Task UpdateContactAsync_ContactDoesNotExistInDb_ReturnsNotFound()
    {
        // Act
        var tenant = TestData.TenantFaker.Generate();
        await _dbContext.Tenants.AddAsync(tenant);
        await _dbContext.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _contactService.UpdateContactAsync(Guid.NewGuid(), new UpdateContactDto());

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT0.Should().BeFalse();
            actualResult.IsT1.Should().BeTrue();
        }
    }

    [Fact]
    public async Task UpdateContactAsync_UpdateContactSuccessfully_ReturnsContactDto()
    {
        // Act
        var tenant = TestData.TenantFaker.Generate();
        var contact = TestData.ContactFaker
            .RuleFor(e => e.TenantId, _ => tenant.Id)
            .Generate();

        await _dbContext.Tenants.AddAsync(tenant);
        await _dbContext.Contacts.AddAsync(contact);
        await _dbContext.SaveChangesAsync();

        var updateContactDto = TestData.UpdateContactFaker.Generate();
        var expectedContact = updateContactDto.Adapt<Contact>();
        expectedContact.TenantId = tenant.Id;

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _contactService.UpdateContactAsync(contact.Id, updateContactDto);

        // Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT0(out var createdContact, out _).Should().BeTrue();
            (await _dbContext.SaveChangesAsync()).Should().Be(0);
            createdContact.Should().BeEquivalentTo(expectedContact, opt =>
            {
                opt.Excluding(x => x.Id);
                opt.Excluding(x => x.CreatedDateTimeUtc);
                opt.Excluding(x => x.Phones);
                opt.ExcludingMissingMembers();
                return opt;
            });
            createdContact.Phones.Should().BeEquivalentTo(updateContactDto.Phones, opt => opt.ExcludingMissingMembers());

            var actualActivity = await _dbContext.ContactActivityLogs.LastOrDefaultAsync();
            actualActivity.Should().NotBeNull();
            actualActivity?.TenantId.Should().Be(tenant.Id);
            actualActivity?.ContactId.Should().Be(contact.Id);
            actualActivity?.EventType.Should().Be(ContactEventType.ContactUpdated);

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Once);
        }
    }
}
