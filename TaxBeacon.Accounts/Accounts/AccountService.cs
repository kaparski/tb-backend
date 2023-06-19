﻿using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;
using System.Collections.Immutable;
using TaxBeacon.Accounts.Accounts.Models;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Interfaces;

namespace TaxBeacon.Accounts.Accounts;

public class AccountService: IAccountService
{
    private readonly ILogger<AccountService> _logger;
    private readonly IAccountDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeService _dateTimeService;
    private readonly IImmutableDictionary<FileType, IListToFileConverter> _listToFileConverters;

    public AccountService(ILogger<AccountService> logger,
        IAccountDbContext context,
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService,
        IEnumerable<IListToFileConverter> listToFileConverters)
    {
        _logger = logger;
        _context = context;
        _currentUserService = currentUserService;
        _dateTimeService = dateTimeService;
        _listToFileConverters = listToFileConverters?.ToImmutableDictionary(x => x.FileType)
                                ?? ImmutableDictionary<FileType, IListToFileConverter>.Empty;
    }

    public IQueryable<AccountDto> QueryAccounts() =>
        _context.AccountsView
            .Where(av => av.TenantId == _currentUserService.TenantId)
            .ProjectToType<AccountDto>();

    public async Task<OneOf<AccountDetailsDto, NotFound>> GetAccountDetailsByIdAsync(Guid id,
        CancellationToken cancellationToken = default)
    {
        var accountDetails = await _context.Accounts
            .Where(a => a.Id == id && a.TenantId == _currentUserService.TenantId)
            .Select(a => new AccountDetailsDto
            {
                Id = a.Id,
                Name = a.Name,
                DoingBusinessAs = a.DoingBusinessAs,
                LinkedInUrl = a.LinkedInUrl,
                Website = a.Website,
                Country = a.Country,
                StreetAddress1 = a.StreetAddress1,
                StreetAddress2 = a.StreetAddress2,
                City = a.City,
                State = a.State,
                Zip = a.Zip,
                County = a.County,
                Phone = a.Phone,
                Extension = a.Extension,
                Fax = a.Fax,
                Address = a.Address,
                EntitiesCount = a.Entities.Count,
                LocationsCount = a.Locations.Count,
                ContactsCount = a.Contacts.Count,
                Client = a.Client == null ? null : new ClientDto(a.Client.State, a.Client.Status),
                Referral = a.Referral == null ? null : new ReferralDto(a.Referral.State, a.Referral.Status),
            })
            .SingleOrDefaultAsync(cancellationToken);

        return accountDetails is not null
            ? accountDetails
            : new NotFound();
    }

    public async Task<byte[]> ExportAccountsAsync(FileType fileType,
        CancellationToken cancellationToken = default)
    {
        var exportAccounts = await _context
            .AccountsView
            .Where(a => a.TenantId == _currentUserService.TenantId)
            .OrderBy(a => a.Name)
            .ProjectToType<AccountExportDto>()
            .ToListAsync(cancellationToken);
        
        _logger.LogInformation("{dateTime} - Accounts export was executed by {@userId}",
            _dateTimeService.UtcNow,
            _currentUserService.UserId);
        
        return _listToFileConverters[fileType].Convert(exportAccounts);
    }
}