namespace TaxBeacon.UserManagement.UnitTests.Services
{
    internal class DepartmentUpdatedEvent
    {
        private Guid id;
        private string v1;
        private string fullName;
        private DateTime utcNow;
        private string v2;
        private string v3;

        public DepartmentUpdatedEvent(Guid id, string v1, string fullName, DateTime utcNow, string v2, string v3)
        {
            this.id = id;
            this.v1 = v1;
            this.fullName = fullName;
            this.utcNow = utcNow;
            this.v2 = v2;
            this.v3 = v3;
        }
    }
}