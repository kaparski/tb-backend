﻿using Gridify;
using Gridify.EntityFramework;
using Mapster;
using TaxBeacon.DAL.Interfaces;
using TaxBeacon.UserManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Exceptions;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Entities;

namespace TaxBeacon.UserManagement.Services;

public class UserService: IUserService
{
    private readonly ILogger<UserService> _logger;
    private readonly ITaxBeaconDbContext _context;
    private readonly IDateTimeService _dateTimeService;

    public UserService(
        ILogger<UserService> logger,
        ITaxBeaconDbContext context,
        IDateTimeService dateTimeService)
    {
        _logger = logger;
        _context = context;
        _dateTimeService = dateTimeService;
    }

    public async Task LoginAsync(MailAddress mailAddress, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => mailAddress.Address == u.Email, cancellationToken);

        if (user is null)
        {
            await CreateUserAsync(
                new User
                {
                    FirstName = string.Empty,
                    LastName = string.Empty,
                    Email = mailAddress.Address,
                    LastLoginDateUtc = _dateTimeService.UtcNow,
                }, cancellationToken);
        }
        else
        {
            user.LastLoginDateUtc = _dateTimeService.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<QueryablePaging<UserDto>> GetUsersAsync(GridifyQuery gridifyQuery,
        CancellationToken cancellationToken)
    {
        var users = await _context
            .Users
            .ProjectToType<UserDto>()
            .GridifyQueryableAsync(gridifyQuery, null, cancellationToken);

        return users;
    }

    public async Task<UserDto> GetUserDetails(Guid id, CancellationToken cancellationToken)
    {
        var user = await _context
            .Users
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken) ?? throw new Exception("User id is not found");

        return user.Adapt<UserDto>();
    }

    private async Task<User> CreateUserAsync(
        User user,
        CancellationToken cancellationToken = default)
    {
        // TODO: This is a temporary solution for tenants, because we will always keep one tenant in db for now
        var tenant = _context.Tenants.First();
        user.UserStatus = UserStatus.Active;

        user.TenantUsers.Add(new TenantUser { Tenant = tenant });
        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return user;
    }
}
