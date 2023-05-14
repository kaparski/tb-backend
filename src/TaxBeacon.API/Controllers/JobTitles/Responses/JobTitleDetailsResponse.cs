namespace TaxBeacon.API.Controllers.JobTitles.Responses;

public class JobTitleDetailsResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime CreatedDateTimeUtc { get; set; }
}
