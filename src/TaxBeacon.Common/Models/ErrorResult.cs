namespace TaxBeacon.Common.Models
{
    public class ErrorResult
    {
        public List<string> Messages { get; set; } = new();
        public string? Name { get; set; }
        public string? Exception { get; set; }
        public string? TraceId { get; set; }
        public int StatusCode { get; set; }
    }
}
