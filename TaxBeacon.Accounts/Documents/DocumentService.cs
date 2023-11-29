using OneOf.Types;
using OneOf;
using TaxBeacon.Common.Services;
using TaxBeacon.Accounts.Documents.Models;
using TaxBeacon.DAL.Accounts;
using Mapster;
using Microsoft.EntityFrameworkCore;
using TaxBeacon.Common.Enums;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using TaxBeacon.Common.Converters;

namespace TaxBeacon.Accounts.Documents;
public class DocumentService: IDocumentService
{
    private readonly IAccountDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeFormatter _dateTimeFormatter;
    private readonly ILogger<DocumentService> _logger;
    private readonly IDateTimeService _dateTimeService;
    private readonly ImmutableDictionary<FileType, IListToFileConverter> _listToFileConverters;

    public DocumentService(IAccountDbContext context, ICurrentUserService currentUserService, IDateTimeFormatter dateTimeFormatter,
        ILogger<DocumentService> logger, IDateTimeService dateTimeService, IEnumerable<IListToFileConverter> listToFileConverters)
    {
        _context = context;
        _currentUserService = currentUserService;
        _dateTimeFormatter = dateTimeFormatter;
        _logger = logger;
        _dateTimeService = dateTimeService;
        _listToFileConverters = listToFileConverters?.ToImmutableDictionary(x => x.FileType)
                               ?? ImmutableDictionary<FileType, IListToFileConverter>.Empty;
    }

    public OneOf<IQueryable<AccountDocumentDto>, NotFound> QueryDocuments(Guid accountId)
    {
        var tenantId = _currentUserService.TenantId;

        var accountExists = _context.Accounts.Any(acc => acc.Id == accountId && acc.TenantId == tenantId);

        if (!accountExists)
        {
            return new NotFound();
        }

        var documents = _context.AccountDocuments
            .Where(d => d.AccountId == accountId && d.TenantId == tenantId)
            .ProjectToType<AccountDocumentDto>();
        return OneOf<IQueryable<AccountDocumentDto>, NotFound>.FromT0(documents);

    }

    public async Task<OneOf<byte[], NotFound>> ExportDocumentsAsync(Guid accountId, FileType fileType, CancellationToken cancellationToken = default)
    {
        var isAccountExists = await _context.Accounts
            .AnyAsync(a => a.Id == accountId && a.TenantId == _currentUserService.TenantId,
                cancellationToken);

        if (!isAccountExists)
        {
            return new NotFound();
        }

        var documents = await _context.AccountDocuments
            .Where(d => d.TenantId == _currentUserService.TenantId && d.AccountId == accountId)
            .OrderBy(d => d.Document.Name)
            .ProjectToType<DocumentExportModel>()
            .ToListAsync(cancellationToken);

        documents.ForEach(d => d.CreatedDateView = _dateTimeFormatter.FormatDate(d.CreatedDateTimeUtc));

        _logger.LogInformation("{dateTime} - Documents export was executed by {userId}",
            _dateTimeService.UtcNow,
            _currentUserService.UserId);

        return _listToFileConverters[fileType].Convert(documents);
    }
}
