using FluentAssertions;
using FluentAssertions.Collections;
using TaxBeacon.Accounts.Common;
using TaxBeacon.Accounts.Common.Models;
using TaxBeacon.Accounts.Extensions;

namespace TaxBeacon.Accounts.UnitTests.Extensions;

public static class PhonesShouldBeOrderedByTypeExtension
{
    public static AndConstraint<GenericCollectionAssertions<T>> ShouldBeOrderedByPhoneType<T>(
        this IEnumerable<T> actualValue) where T : PhoneDto
    {
        var phoneDtos = actualValue.ToList();
        var tempCollection = phoneDtos.OrderByPhoneType();

        return phoneDtos.Should().ContainInConsecutiveOrder(tempCollection);
    }
}
