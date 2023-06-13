using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Interfaces;

namespace TaxBeacon.Accounts.Entities;

public class EntityService: IEntityService
{
    private readonly ILogger<EntityService> _logger;
    private readonly IAccountDbContext _context;
    private readonly IDateTimeService _dateTimeService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IImmutableDictionary<FileType, IListToFileConverter> _listToFileConverters;
    public EntityService(ILogger<EntityService> logger,
        IAccountDbContext context,
        IDateTimeService dateTimeService,
        ICurrentUserService currentUserService,
        IEnumerable<IListToFileConverter> listToFileConverters)
    {
        _logger = logger;
        _context = context;
        _dateTimeService = dateTimeService;
        _currentUserService = currentUserService;
        _listToFileConverters = listToFileConverters?.ToImmutableDictionary(x => x.FileType)
                                ?? ImmutableDictionary<FileType, IListToFileConverter>.Empty;
    }

    public IQueryable<EntityDto> QueryEntities()
    {
        var items = _context.Entities.Where(d => d.TenantId == _currentUserService.TenantId);

        var itemDtos = items.Select(d => new EntityDto()
        {
            Id = d.Id,
            Name = d.Name,
            City = d.City,
            State = d.State,
            Type = d.Type,
            Status = d.Status,
            EntityId = d.EntityId
        });

        return itemDtos;
    }
}
