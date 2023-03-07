namespace TaxBeacon.UserManagement.Services
{
    public interface IPasswordGenerator
    {
        /// <summary>
        /// Generates new password.
        /// </summary>
        /// <returns></returns>
        string GeneratePassword();
    }
}
