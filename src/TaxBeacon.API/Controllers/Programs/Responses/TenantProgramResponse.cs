namespace TaxBeacon.API.Controllers.Programs.Responses;

public class TenantProgramResponse
{
    public Guid Id { get; set; }

    public string Reference { get; set; }

    public string Name { get; set; }

    public Jurisdiction Jurisdiction { get; set; }

    public string JurisdictionName { get; set; }

    public IncentivesArea IncentivesArea { get; set; }

    public IncentivesType IncentivesType { get; set; }

    public ProgramStatus Status { get; set; }

    public DateTime StartDateTimeUtc { get; set; }

    public DateTime EndDateTimeUtc { get; set; }

    public DateTime CreatedDateTimeUtc { get; set; }
}

public enum IncentivesArea
{
    IncomeTax = 1,
    PayrollTax = 2,
}

public enum IncentivesType
{
    TaxCredit = 1,
    Deduction = 2,
}

public enum ProgramStatus
{
    Active = 1,
    Inactive = 2,
}

public enum Jurisdiction
{
    Federal = 1,
    State = 2,
    Local = 3,
}

