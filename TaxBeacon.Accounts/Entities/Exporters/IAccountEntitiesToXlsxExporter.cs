﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Entities.Exporters;
public interface IAccountEntitiesToXlsxExporter
{
    FileStreamDto Export(AccountEntitiesExportModel model);
}