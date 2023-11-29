using Mapster;
using TaxBeacon.DAL.Documents.Entities;

namespace TaxBeacon.Accounts.Documents.Models;
public record AccountDocumentDto: IRegister
{
    public Guid Id { get; init; }

    public Guid UploadedById { get; init; }

    public string UploadedByFullName { get; init; } = null!;

    public string Name { get; init; } = null!;

    public DateTime CreatedDateTimeUtc { get; init; }

    public void Register(TypeAdapterConfig config) =>
    config.NewConfig<AccountDocument, AccountDocumentDto>()
        .Map(dest => dest.Id, src => src.DocumentId)
        .Map(dest => dest.UploadedById, src => src.Document.UserId)
        .Map(dest => dest.Name, src => src.Document.Name)
        .Map(dest => dest.CreatedDateTimeUtc, src => src.Document.CreatedDateTimeUtc)
        .Map(dest => dest.UploadedByFullName, src => src.Document.TenantUser.User.FullName);
}
