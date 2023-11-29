using OneOf;
using OneOf.Types;
using TaxBeacon.Accounts.Entities.Models;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Errors;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Entities;

public interface IEntityService
{
    OneOf<IQueryable<EntityDto>, NotFound> QueryEntitiesAsync(Guid accountId);

    Task<OneOf<ActivityDto, NotFound>> GetActivitiesAsync(Guid entityId,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    Task<OneOf<EntityDetailsDto, NotFound>> GetEntityDetailsAsync(Guid entityId,
        CancellationToken cancellationToken = default);

    Task<OneOf<EntityDetailsDto, NotFound, InvalidOperation>> UpdateEntityAsync(Guid id,
        UpdateEntityDto updateEntity,
        CancellationToken cancellationToken = default);

    Task<OneOf<EntityDetailsDto, NotFound>> UpdateEntityStatusAsync(Guid entityId,
        Status status,
        CancellationToken cancellationToken = default);

    Task<OneOf<FileStreamDto, NotFound>> ExportAccountEntitiesAsync(Guid accountId,
        FileType fileType,
        CancellationToken cancellationToken = default);

    Task<OneOf<EntityDetailsDto, NotFound, InvalidOperation>> CreateNewEntityAsync(Guid accountId,
        CreateEntityDto createEntityDto,
        CancellationToken cancellationToken = default);

    IQueryable<StateIdDto> GetEntityStateIdsAsync(Guid entityId);

    Task<OneOf<Success, NotFound>> RemoveStateIdFromEntityAsync(Guid entityId,
        Guid stateId,
        CancellationToken cancellationToken = default);

    Task<OneOf<List<StateIdDto>, NotFound>> AddStateIdsAsync(Guid entityId,
        List<AddStateIdDto> newStateIds,
        CancellationToken cancellationToken = default);

    Task<OneOf<StateIdDto, NotFound>> UpdateStateIdAsync(Guid entityId,
        Guid stateId,
        UpdateStateIdDto updateStateIdDto,
        CancellationToken cancellationToken = default);

    Task<OneOf<string, InvalidOperation>> GenerateUniqueEntityIdAsync(CancellationToken cancellationToken);
    
    Task<OneOf<Success, InvalidOperation>> ImportAccountEntitiesAsync(Guid accountId,
        Stream stream,
        CancellationToken cancellationToken);
}
