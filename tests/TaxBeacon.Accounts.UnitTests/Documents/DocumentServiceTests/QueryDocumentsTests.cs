using FluentAssertions.Execution;
using FluentAssertions;
using Mapster;
using OneOf.Types;
using TaxBeacon.Accounts.Documents.Models;
using TaxBeacon.DAL.Administration.Entities;
using TaxBeacon.DAL.Documents.Entities;

namespace TaxBeacon.Accounts.UnitTests.Documents;
public partial class DocumentServiceTests
{
    [Fact]
    public async Task QueryDocuments_AccountExists_ReturnsDocuments()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(acc => acc.Tenant, f => tenant)
            .Generate();
        var user = TestData.UserFaker.Generate();
        var tenantUser = new TenantUser
        {
            TenantId = tenant.Id,
            UserId = user.Id,
            User = user
        };
        var documents = TestData.DocumentFaker
            .RuleFor(d => d.TenantId, f => tenant.Id)
            .RuleFor(d => d.TenantUser, f => tenantUser)
            .RuleFor(d => d.UserId, f => tenantUser.UserId)
            .Generate(5)
            .Select(x => new AccountDocument
            {
                Tenant = tenant,
                TenantId = tenant.Id,
                DocumentId = x.Id,
                Document = x,
                Account = account,
                AccountId = account.Id
            });

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddRangeAsync(account);
        await _dbContextMock.AccountDocuments.AddRangeAsync(documents);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(tenant.Id);

        // Act
        var oneOf = _documentService.QueryDocuments(account.Id);

        // Assert
        using (new AssertionScope())
        {
            oneOf.TryPickT0(out var AccountDocumentsDtos, out _).Should().BeTrue();
            AccountDocumentsDtos.Should().HaveCount(5);

            foreach (var dto in AccountDocumentsDtos)
            {
                var accountDocument = AccountDocumentsDtos.Single(u => u.Id == dto.Id);

                dto.Should().BeEquivalentTo(accountDocument.Adapt<AccountDocumentDto>());
            }
        }
    }

    [Fact]
    public async Task QueryDocuments_AccountDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(acc => acc.Tenant, f => tenant)
            .Generate();
        var user = TestData.UserFaker.Generate();
        var tenantUser = new TenantUser
        {
            TenantId = tenant.Id,
            UserId = user.Id,
            User = user
        };
        var documents = TestData.DocumentFaker
            .RuleFor(d => d.TenantId, f => tenant.Id)
            .RuleFor(d => d.TenantUser, f => tenantUser)
            .RuleFor(d => d.UserId, f => tenantUser.UserId)
            .Generate(5)
            .Select(x => new AccountDocument
            {
                Tenant = tenant,
                TenantId = tenant.Id,
                DocumentId = x.Id,
                Document = x,
                Account = account,
                AccountId = account.Id
            });

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddRangeAsync(account);
        await _dbContextMock.AccountDocuments.AddRangeAsync(documents);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(tenant.Id);

        // Act
        var oneOf = _documentService.QueryDocuments(Guid.NewGuid());

        // Assert
        using (new AssertionScope())
        {
            oneOf.TryPickT1(out var notFound, out _).Should().BeTrue();
            notFound.Should().BeOfType<NotFound>();
        }
    }
}
