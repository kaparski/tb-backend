using TaxBeacon.Accounts.Common;
using TaxBeacon.Accounts.Common.Models;

namespace TaxBeacon.Accounts.Extensions;

public static class OrderPhonesByTypeExtension
{
    public static IEnumerable<T> OrderByPhoneType<T>(this IEnumerable<T> phones) where T : PhoneDto
        => phones.OrderBy(p => p.Type).ThenBy(p => p.CreatedDateTimeUtc);
}
