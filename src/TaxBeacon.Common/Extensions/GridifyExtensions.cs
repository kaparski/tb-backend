using Gridify;

namespace TaxBeacon.Common.Extensions;

public static class GridifyExtensions
{
    public static List<bool> GetOrderings(this GridifyQuery query)
    {
        var orderings = new List<bool>();
        if (query.OrderBy == null)
        {
            return orderings;
        }

        foreach (var field in query.OrderBy.Split(','))
        {
            var orderingExp = field.Trim();
            var isAsc = true;
            if (orderingExp.Contains(" "))
            {
                var spliced = orderingExp.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                isAsc = spliced.Last() switch
                {
                    "desc" => false,
                    "asc" => true,
                    _ => throw new GridifyOrderingException("Invalid keyword. expected 'desc' or 'asc'")
                };
            }

            orderings.Add(isAsc);
        }

        return orderings;
    }
}
