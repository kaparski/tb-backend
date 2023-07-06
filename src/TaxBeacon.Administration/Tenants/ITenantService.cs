using OneOf;
using OneOf.Types;
using TaxBeacon.Administration.Tenants.Models;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Administration.Tenants;

public interface ITenantService
{
    IQueryable<TenantDto> QueryTenants();

    Task<byte[]> ExportTenantsAsync(FileType fileType, CancellationToken cancellationToken);

    Task<OneOf<TenantDto, NotFound>> GetTenantByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<OneOf<ActivityDto, NotFound>> GetActivityHistoryAsync(Guid id,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<OneOf<TenantDto, NotFound>> UpdateTenantAsync(Guid id,
        UpdateTenantDto updateTenantDto,
        CancellationToken cancellationToken);

    Task<TenantDto?> SwitchToTenantAsync(Guid? oldTenantId, Guid? newTenantId,
        CancellationToken cancellationToken = default);

    Task<OneOf<Success, NotFound>> ToggleDivisionsAsync(bool divisionEnabled, CancellationToken cancellationToken);

    Task<OneOf<List<AssignedTenantProgramDto>, NotFound>> GetTenantProgramsAsync(Guid tenantId,
        CancellationToken cancellationToken);

    Task<OneOf<Success, NotFound>> ChangeTenantProgramsAsync(Guid tenantId,
        IEnumerable<Guid> programsIds,
        CancellationToken cancellationToken);
}
