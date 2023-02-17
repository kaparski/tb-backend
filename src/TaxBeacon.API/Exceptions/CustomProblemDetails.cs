using Microsoft.AspNetCore.Mvc;

namespace TaxBeacon.API.Exceptions;

public class CustomProblemDetails: ProblemDetails
{
    public int ErrorCode { get; set; }
}
