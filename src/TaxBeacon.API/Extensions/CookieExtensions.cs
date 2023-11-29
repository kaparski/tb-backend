using System.Text;

namespace TaxBeacon.API.Extensions;

public static class CookieExtensions
{
    public static void WriteChunkedValue(this IResponseCookies cookies,
        string cookieName,
        string value,
        CookieOptions options)
    {
        const int chunkSize = 4000;
        var spanValue = value.AsSpan();

        for (var i = 0; i < value.Length; i += chunkSize)
        {
            var chunk = spanValue.Slice(i, Math.Min(chunkSize, spanValue.Length - i));
            cookies.Append($"{cookieName}{i / chunkSize}", chunk.ToString(), options);
        }
    }

    public static string GetChunkedValue(this IRequestCookieCollection cookies, string cookieName)
    {
        var resulValue = new StringBuilder();
        var chunkIndex = 0;

        while (true)
        {
            var chunk = cookies[$"{cookieName}{chunkIndex}"];
            if (chunk is null)
            {
                break;
            }

            resulValue.Append(chunk);
            chunkIndex++;
        }

        return resulValue.ToString();
    }
}
