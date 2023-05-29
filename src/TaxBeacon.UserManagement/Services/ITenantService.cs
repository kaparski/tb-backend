﻿using Gridify;
using OneOf;
using OneOf.Types;
using TaxBeacon.Common.Enums;
using TaxBeacon.UserManagement.Models;

namespace TaxBeacon.UserManagement.Services;

public interface ITenantService
{
    Task<QueryablePaging<TenantDto>> GetTenantsAsync(GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default);

    Task<byte[]> ExportTenantsAsync(FileType fileType, CancellationToken cancellationToken);

    Task<OneOf<TenantDto, NotFound>> GetTenantByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<OneOf<ActivityDto, NotFound>> GetActivityHistoryAsync(Guid id, int page, int pageSize, CancellationToken cancellationToken);

    Task<OneOf<TenantDto, NotFound>> UpdateTenantAsync(Guid id, UpdateTenantDto updateTenantDto,
        CancellationToken cancellationToken);

    Task SwitchToTenantAsync(Guid? oldTenantId, Guid? newTenantId, CancellationToken cancellationToken = default);

    Task<OneOf<Success, NotFound>> ToggleDivisionsAsync(bool divisionEnabled, CancellationToken cancellationToken);
}
