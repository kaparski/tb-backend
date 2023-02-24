namespace TaxBeacon.UserManagement.Models;

public class PageDto<T>
{
    public int Count { get; set; }

    public IEnumerable<T> Query { get; set; }
}
