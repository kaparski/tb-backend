﻿using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics.CodeAnalysis;
using TaxBeacon.Accounts.Services.Contacts;
using TaxBeacon.Common.Accounts;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL;
using TaxBeacon.DAL.Entities;
using TaxBeacon.DAL.Entities.Accounts;
using TaxBeacon.DAL.Interceptors;
using TaxBeacon.DAL.Interfaces;

namespace TaxBeacon.Accounts.UnitTests.Services;

public class ContactServiceTests
{
    private readonly ITaxBeaconDbContext _dbContextMock;
    private readonly IContactService _contactService;
    private readonly Mock<EntitySaveChangesInterceptor> _entitySaveChangesInterceptorMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;

    public ContactServiceTests()
    {
        _currentUserServiceMock = new();
        _entitySaveChangesInterceptorMock = new();
        _dbContextMock = new TaxBeaconDbContext(
            new DbContextOptionsBuilder<TaxBeaconDbContext>()
                .UseInMemoryDatabase($"{nameof(ContactServiceTests)}-InMemoryDb-{Guid.NewGuid()}")
                .Options,
            _entitySaveChangesInterceptorMock.Object);
        _contactService = new ContactService(_currentUserServiceMock.Object, _dbContextMock);

    }

    [Fact]
    public async Task QueryDepartments_CorrectArguments_ReturnsContacts()
    {
        // Arrange
        var tenant = TestData.TestTenant.Generate();
        await _dbContextMock.Tenants.AddAsync(tenant);
        TestData.TestContact.RuleFor(x => x.Tenant, tenant);
        _currentUserServiceMock.Setup(x => x.TenantId).Returns(tenant.Id);
        var items = TestData.TestContact.Generate(3);
        await _dbContextMock.Contacts.AddRangeAsync(items);
        await _dbContextMock.SaveChangesAsync();

        // Act
        var query = _contactService.QueryContacts();
        var result = query.ToArray();

        // Assert
        using (new AssertionScope())
        {
            result.Should().HaveCount(3);

            foreach (var dto in result)
            {
                var item = items.Single(u => u.Id == dto.Id);

                dto.Should().BeEquivalentTo(item, opt => opt.ExcludingMissingMembers());
            }
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private static class TestData
    {
        public static readonly Guid TestTenantId = Guid.NewGuid();

        public static readonly Faker<Contact> TestContact =
            new Faker<Contact>()
                .RuleFor(t => t.Id, f => Guid.NewGuid())
                .RuleFor(t => t.Account, t => new Account()
                {
                    Name = t.Name.JobArea(),
                    Website = t.Name.JobTitle()
                })
                .RuleFor(t => t.FirstName, f => f.Person.FirstName)
                .RuleFor(t => t.LastName, f => f.Person.LastName)
                .RuleFor(t => t.Email, t => t.Person.Email)
                .RuleFor(t => t.Type, t => t.PickRandom("Client", "Referral Partner", "Client Partner"))
                .RuleFor(t => t.CreatedDateTimeUtc, f => DateTime.UtcNow)
                .RuleFor(t => t.Status, t => Status.Active);

        public static readonly Faker<Tenant> TestTenant =
            new Faker<Tenant>()
                .RuleFor(t => t.Id, f => TestTenantId)
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.CreatedDateTimeUtc, f => DateTime.UtcNow);
    }
}
