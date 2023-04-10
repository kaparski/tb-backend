using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Interfaces;

namespace TaxBeacon.UserManagement.Services
{
    public class PermissionsService: IPermissionsService
    {
        private readonly ILogger<PermissionsService> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly ITaxBeaconDbContext _context;

        public PermissionsService(ITaxBeaconDbContext context,
            ILogger<PermissionsService> logger,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _logger = logger;
            _currentUserService = currentUserService;
        }
    }
}
