using TaxBeacon.Accounts.Naics.Models;

namespace TaxBeacon.Accounts.Naics;
public interface INaicsService
{
    Task<List<NaicsCodeTreeItemDto>> GetNaicsCodesAsync(CancellationToken cancellationToken = default);

    Task<bool> IsNaicsValidAsync(int? primaryNaics, CancellationToken cancellationToken);
}
