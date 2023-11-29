using Microsoft.AspNetCore.Http;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.DocumentManagement.BlobStorage.Models;

public record UploadFileDto(Guid DocumentId, EntityType EntityType, IFormFile File);
