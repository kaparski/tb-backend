namespace TaxBeacon.Administration.PasswordGenerator;

public interface IPasswordGenerator
{
    /// <summary>
    /// Generates new password.
    /// </summary>
    /// <returns></returns>
    string GeneratePassword();
}
