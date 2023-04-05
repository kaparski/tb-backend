namespace TaxBeacon.API.Controllers.Users.Responses
{
    public record UserActivityResponse(uint Count, IEnumerable<UserActivityItemResponse> Query);

    public record UserActivityItemResponse(string Message, DateTime Date, string FullName);

}
