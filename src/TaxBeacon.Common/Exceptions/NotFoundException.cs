
using System.Net;

namespace TaxBeacon.Common.Exceptions;

public class NotFoundException: CustomException
{
    public NotFoundException(string propertyName) : base($"{propertyName} not found", null, HttpStatusCode.NotFound) { }
}
