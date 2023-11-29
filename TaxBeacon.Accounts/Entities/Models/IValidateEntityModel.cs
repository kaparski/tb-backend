using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxBeacon.Accounts.Entities.Models;
public interface IValidateEntityModel
{
    public string Name { get; init; }

    public string EntityId { get; init; }

    public string? Fein { get; init; }

    public string? Ein { get; init; }
}
