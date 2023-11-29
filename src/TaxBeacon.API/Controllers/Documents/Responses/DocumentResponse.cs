using TaxBeacon.DAL.Documents.Entities;

namespace TaxBeacon.API.Controllers.Documents.Responses;

public record DocumentResponse
{
    public Guid Id { get; init; }

    public Guid UploadedById { get; init; }

    public string UploadedByFullName { get; init; } = null!;

    public string Name { get; init; } = null!;

    public DateTime CreatedDateTimeUtc { get; init; }
}
