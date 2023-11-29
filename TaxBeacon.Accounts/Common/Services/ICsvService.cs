namespace TaxBeacon.Accounts.Common.Services;

public interface ICsvService
{
    IEnumerable<T> Read<T>(Stream stream);
}
