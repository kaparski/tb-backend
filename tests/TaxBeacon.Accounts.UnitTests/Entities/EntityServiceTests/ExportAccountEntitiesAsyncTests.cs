using Moq;
using TaxBeacon.Accounts.Entities.Exporters;
using TaxBeacon.Common.Enums;
using FluentAssertions;

namespace TaxBeacon.Accounts.UnitTests.Entities;
public partial class EntityServiceTests
{
    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public async Task ExportAccountEntitiesAsync_ValidInputData_AppropriateConverterShouldBeCalled(FileType fileType)
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var entities = TestData.EntityFaker
            .RuleFor(e => e.TenantId, _ => tenant.Id)
            .RuleFor(e => e.AccountId, _ => account.Id)
            .Generate(5);

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Entities.AddRangeAsync(entities);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);

        //Act
        _ = await _entityService.ExportAccountEntitiesAsync(account.Id, fileType);

        //Assert
        switch (fileType)
        {
            case FileType.Csv:
                _accountEntitiesToCsvExporter.Verify(
                    x => x.Export(It.IsAny<AccountEntitiesExportModel>(), It.IsAny<CancellationToken>()), Times.Once());
                break;
            case FileType.Xlsx:
                _accountEntitiesToXlsxExporter.Verify(x => x.Export(It.IsAny<AccountEntitiesExportModel>()),
                    Times.Once());
                break;
            default:
                throw new InvalidOperationException();
        }
    }

    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public async Task ExportAccountEntitiesAsync_AccountDoesNotExist_ReturnsNotFound(FileType fileType)
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var entities = TestData.EntityFaker
            .RuleFor(e => e.TenantId, _ => tenant.Id)
            .RuleFor(e => e.AccountId, _ => account.Id)
            .Generate(5);

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Entities.AddRangeAsync(entities);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);

        //Act
        var result = await _entityService.ExportAccountEntitiesAsync(Guid.NewGuid(), fileType);

        //Assert
        result.IsT0.Should().BeFalse();
        result.IsT1.Should().BeTrue();
    }

    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public async Task ExportAccountEntitiesAsync_AccountFromAnotherTenant_ReturnsNotFound(FileType fileType)
    {
        //Arrange
        var tenants = TestData.TenantFaker.Generate(2);
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenants[0].Id)
            .Generate();
        var entities = TestData.EntityFaker
            .RuleFor(e => e.TenantId, _ => tenants[0].Id)
            .RuleFor(e => e.AccountId, _ => account.Id)
            .Generate(5);

        await _dbContextMock.Tenants.AddRangeAsync(tenants);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Entities.AddRangeAsync(entities);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenants[1].Id);

        //Act
        var result = await _entityService.ExportAccountEntitiesAsync(account.Id, fileType);

        //Assert
        result.IsT0.Should().BeFalse();
        result.IsT1.Should().BeTrue();
    }
}
