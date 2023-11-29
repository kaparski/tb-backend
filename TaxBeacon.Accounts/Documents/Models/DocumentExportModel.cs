using Mapster;
using Npoi.Mapper.Attributes;
using TaxBeacon.DAL.Documents.Entities;

namespace TaxBeacon.Accounts.Documents.Models;
public class DocumentExportModel: IRegister
{

    [Column("Document Name")]
    public string Name { get; init; } = null!;

    [Ignore]
    public DateTime CreatedDateTimeUtc { get; init; }

    [Column("Upload Date")]
    public string CreatedDateView { get; set; } = string.Empty;

    [Column("Uploaded by")]
    public string UserFullName { get; init; } = null!;

    public void Register(TypeAdapterConfig config) =>
        config.NewConfig<AccountDocument, DocumentExportModel>()
            .Map(dest => dest.Name, src => src.Document.Name)
            .Map(dest => dest.CreatedDateTimeUtc, src => src.Document.CreatedDateTimeUtc)
            .Map(dest => dest.UserFullName, src => src.Document.TenantUser.User.FullName);
}
