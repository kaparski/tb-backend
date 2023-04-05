namespace TaxBeacon.UserManagement.Models.Activities
{
    public abstract class UserEventBase
    {
        protected UserEventBase(string executorRoles, string executorFullName)
        {
            ExecutorRoles = executorRoles;
            ExecutorFullName = executorFullName;
        }

        public string ExecutorRoles { get; }

        public string ExecutorFullName { get; }
    }
}
