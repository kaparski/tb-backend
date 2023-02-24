using Gridify;
using Gridify.EntityFramework;
using Mapster;
using TaxBeacon.DAL.Interfaces;
using TaxBeacon.UserManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Exceptions;
using TaxBeacon.Common.Extensions;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Entities;

namespace TaxBeacon.UserManagement.Services;

public class UserService: IUserService
{
    private readonly ILogger<UserService> _logger;
    private readonly ITaxBeaconDbContext _context;
    private readonly IDateTimeService _dateTimeService;
    private readonly ICurrentUserService _currentUserService;

    public UserService(
        ILogger<UserService> logger,
        ITaxBeaconDbContext context,
        IDateTimeService dateTimeService,
        ICurrentUserService currentUserService)
    {
        _logger = logger;
        _context = context;
        _dateTimeService = dateTimeService;
        _currentUserService = currentUserService;
    }

    public async Task LoginAsync(MailAddress mailAddress, CancellationToken cancellationToken = default)
    {
        var user = await _context
            .Users
            .FirstOrDefaultAsync(u => mailAddress.Address == u.Email, cancellationToken);

        if (user is null)
        {
            await CreateUserAsync(
                new UserDto
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

    public PageDto<UserDto> GetUsers(GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default)
    {
        var users = _context
            .Users
            .Include(x => x.TenantUsers)
                .ThenInclude(x => x.RoleTenantUsers)
                    .ThenInclude(x => x.Role)
            .ApplyPaging(gridifyQuery.Page, gridifyQuery.PageSize)
            .ToList();

        var userDtos = users.Select(x => new UserDto()
        {
            Id = x.Id,
            FirstName = x.FirstName,
            LastName = x.LastName,
            FullName = x.FullName,
            CreatedDateUtc = x.CreatedDateUtc,
            Email = x.Email,
            UserStatus = x.UserStatus,
            LastLoginDateUtc = x.LastLoginDateUtc,
            DeactivationDateTimeUtc = x.DeactivationDateTimeUtc,
            ReactivationDateTimeUtc = x.ReactivationDateTimeUtc,
            Roles = x.TenantUsers.FirstOrDefault() == null ? new List<RoleDto>() :
                x.TenantUsers.FirstOrDefault()
                    ?.RoleTenantUsers
                        .OrderBy(x => x.Role.Name)
                        .Select(x => new RoleDto
                        {
                            Id = x.Role.Id,
                            Name = x.Role.Name
                        }),
        });

        if (!string.IsNullOrWhiteSpace(gridifyQuery.Filter))
        {
            var filterExpression = gridifyQuery.GetFilteringExpression<UserDto>();
            userDtos = userDtos.Where(filterExpression.Compile());
        }

        var orderedUsers = userDtos.OrderBy(x => x.Email);

        if (!string.IsNullOrWhiteSpace(gridifyQuery.OrderBy))
        {
            var orderingExpressions = gridifyQuery.GetOrderingExpressions<UserDto>().ToList();
            var orderings = gridifyQuery.GetOrderings();
            for (var i = 0; i < orderingExpressions.Count; i++)
            {
                orderedUsers = orderings[i]
                    ? orderedUsers.OrderBy(orderingExpressions[i].Compile())
                    : orderedUsers.OrderByDescending(orderingExpressions[i].Compile());
            }
        }

        return new PageDto<UserDto>()
        {
            Count = _context.Users.Count(),
            Query = orderedUsers.AsEnumerable(),
        };
    }

    public async Task<UserDto> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context
            .Users
            .ProjectToType<UserDto>()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
        ?? throw new NotFoundException(nameof(User), id);

    public async Task<UserDto> GetUserByEmailAsync(MailAddress mailAddress,
        CancellationToken cancellationToken = default) =>
        await _context
            .Users
            .ProjectToType<UserDto>()
            .FirstOrDefaultAsync(x => x.Email == mailAddress.Address, cancellationToken)
        ?? throw new NotFoundException(nameof(User), mailAddress.Address);

    public async Task<UserDto> UpdateUserStatusAsync(Guid id, UserStatus userStatus,
        CancellationToken cancellationToken = default)
    {
        // TODO: Move the same code into separated method
        var user = await _context
                       .Users
                       .FirstOrDefaultAsync(user => user.Id == id, cancellationToken)
                   ?? throw new NotFoundException(nameof(User), id);

        switch (userStatus)
        {
            case UserStatus.Deactivated:
                user.DeactivationDateTimeUtc = _dateTimeService.UtcNow;
                user.ReactivationDateTimeUtc = null;
                user.UserStatus = UserStatus.Deactivated;
                break;
            case UserStatus.Active:
                user.ReactivationDateTimeUtc = _dateTimeService.UtcNow;
                user.DeactivationDateTimeUtc = null;
                user.UserStatus = UserStatus.Active;
                break;
        }

        user.UserStatus = userStatus;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{dateTime} - User ({createdUserId}) status was changed to {newUserStatus} by {@userId}",
            _dateTimeService.UtcNow,
            user.Id,
            userStatus,
            _currentUserService.UserId);

        return user.Adapt<UserDto>();
    }

    public HashSet<PermissionEnum> GetUserPermissionsByEmail(string email) => _context.Users
            .Include(x => x.TenantUsers)
                .ThenInclude(x => x.RoleTenantUsers)
                .ThenInclude(x => x.Role)
                .ThenInclude(x => x.RoleTenantPermissions)
                .ThenInclude(x => x.TenantPermission)
                .ThenInclude(x => x.Permission)
            .Where(x => x.Email == email)
            .Select(x => x.TenantUsers)
            .First()
            .SelectMany(x => x.RoleTenantUsers)
            .Select(x => x.Role)
            .SelectMany(x => x.RoleTenantPermissions)
            .Select(x => x.TenantPermission)
            .Select(x => x.Permission)
            .Select(x => x.Name)
            .ToHashSet();

    public async Task<UserDto> CreateUserAsync(
        UserDto newUserData,
        CancellationToken cancellationToken = default)
    {
        // TODO: This is a temporary solution for tenants, because we will always keep one tenant in db for now
        var user = newUserData.Adapt<User>();
        var tenant = _context.Tenants.First();
        user.UserStatus = UserStatus.Active;

        if (await EmailExistsAsync(user.Email, cancellationToken))
        {
            throw new ConflictException(ConflictExceptionMessages.EmailExistsMessage,
                ConflictExceptionKey.UserEmail);
        }

        user.TenantUsers.Add(new TenantUser { Tenant = tenant });
        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{dateTime} - User ({createdUserId}) was created by {@userId}",
            _dateTimeService.UtcNow,
            user.Id,
            _currentUserService.UserId);

        return user.Adapt<UserDto>();
    }

    private async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default) =>
        await _context.Users.AnyAsync(x => x.Email == email, cancellationToken);
}
