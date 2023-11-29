using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Entities.Exporters;
public interface IAccountEntitiesToCsvExporter
{
    Task<FileStreamDto> Export(AccountEntitiesExportModel model, CancellationToken cancellationToken);
}
