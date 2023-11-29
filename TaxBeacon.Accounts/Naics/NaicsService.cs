using Mapster;
using Microsoft.EntityFrameworkCore;
using TaxBeacon.Accounts.Naics.Models;
using TaxBeacon.DAL.Accounts;

namespace TaxBeacon.Accounts.Naics;
public class NaicsService: INaicsService
{
    private readonly IAccountDbContext _context;

    public NaicsService(IAccountDbContext context) => _context = context;

    public async Task<List<NaicsCodeTreeItemDto>> GetNaicsCodesAsync(CancellationToken cancellationToken = default)
    {
        // Cannot use ProjectToType because EF will generate a query with multiple joins
        // To avoid that, all NAICS codes get fetched from the DB and then get filtered and mapped to a DTOs
        var codes = await _context.NaicsCodes
                 .Where(nc => nc.IsDeleted != true)
                 .ToListAsync(cancellationToken);

        // The tree structure remains based on references since all codes were fetched in the query above
        return codes.Where(c => c.ParentCode == null).Adapt<List<NaicsCodeTreeItemDto>>();
    }

    public async Task<bool> IsNaicsValidAsync(int? primaryNaics, CancellationToken cancellationToken) => await _context.NaicsCodes.AnyAsync(
                a => a.Code == primaryNaics, cancellationToken);
}
