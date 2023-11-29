using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaxBeacon.API.Authentication;

namespace TaxBeacon.API.Controllers.LoadTesting;

public class AuthController: BaseController
{
    private readonly LoadTestingOptions _options;

    public AuthController(IOptions<LoadTestingOptions> options) => _options = options.Value;

    [AllowAnonymous]
    [HttpPost]
    public IActionResult Login([FromBody] User user)
    {
        if (_options.IsLoadTestingEnabled != true)
        {
            return NotFound();
        }
        var userToLogin = _options.LoadTestingUsers.FirstOrDefault(u => u.Email == user.Email);

        if (userToLogin == null)
        {
            return NotFound();
        }

        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var signInCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
        var tokenOptions = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: new List<Claim>() { new Claim(Claims.EmailClaimName, userToLogin.Email), new Claim(ClaimTypes.NameIdentifier, userToLogin.ExternalId) },
            expires: DateTime.UtcNow.AddMinutes(_options.TokenLifeTime),
            signingCredentials: signInCredentials
        );
        var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
        return Ok(new { Token = tokenString });
    }
}
