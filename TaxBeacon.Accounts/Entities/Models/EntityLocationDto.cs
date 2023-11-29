using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxBeacon.Accounts.Entities.Models;
public record EntityLocationDto
{
    public Guid EntityId { get; init; }
    public Guid LocationId { get; init; }
}
