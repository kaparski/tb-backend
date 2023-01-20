using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaxBeacon.Common.Enums;
using TaxBeacon.DAL.Entities;
using TaxBeacon.DAL.Interfaces;

namespace TaxBeacon.UserManagement.Services;

public class UserService: IUserService
{
    private readonly ILogger<UserService> _logger;
    private readonly ITaxBeaconDbContext _context;

    public UserService(ILogger<UserService> logger, ITaxBeaconDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task LoginAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u =>
                email.Equals(u.Email, StringComparison.InvariantCultureIgnoreCase),
            cancellationToken);

        if (user is null)
        {
            await CreateUserAsync(
                new User
                {
                    Username = string.Empty,
                    FirstName = string.Empty,
                    LastName = string.Empty,
                    Email = email,
                    LastLoginDateUtc = DateTime.UtcNow
                }, cancellationToken);
        }
        else
        {
            user.LastLoginDateUtc = DateTime.UtcNow;
            _context.Users.Update(user);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<User> CreateUserAsync(
        User user,
        CancellationToken cancellationToken = default)
    {
        var tenant = _context.Tenants.First();
        user.UserStatus = UserStatus.Active;

        user.TenantUsers.Add(new TenantUser { Tenant = tenant });
        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return user;
    }
}
