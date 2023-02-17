using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace TaxBeacon.API.Exceptions;

public class CustomProblemDetails: ProblemDetails
{
    [JsonPropertyName("errorCode")]
    public int ErrorCode { get; set; }
}
