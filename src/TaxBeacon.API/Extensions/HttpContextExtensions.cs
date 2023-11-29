namespace TaxBeacon.API.Extensions;

public static class HttpContextExtensions
{
    public static string GetDomainName(this HttpContext context)
    {
        var host = context.Request.Host.Host;
        var splitHostname = host.Split('.');

        return splitHostname.Length > 1 ? $".{string.Join(".", splitHostname.Skip(1))}" : host;
    }

    /// <summary>
    /// Retrieves GUID id with the specified name from route
    /// </summary>
    /// <param name="context"></param>
    /// <param name="idName"></param>
    /// <returns>Parsed Guid Id from route</returns>
    public static bool TryGetIdFromRoute(this HttpContext? context, string idName, out Guid? id)
    {
        var routeData = context?.GetRouteData();

        if (routeData is not null
            && routeData.Values.TryGetValue(idName, out var routeId)
            && Guid.TryParse(routeId?.ToString(), out var parsedId))
        {
            id = parsedId;
            return true;
        }

        id = null;
        return false;
    }
}
