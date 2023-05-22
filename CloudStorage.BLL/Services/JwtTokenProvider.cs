using System.Security.Claims;
using System.Text;
using CloudStorage.BLL.Options;
using CloudStorage.BLL.Services.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CloudStorage.BLL.Services;

public class JwtTokenProvider : IJwtTokenProvider
{
    private readonly SecurityTokenHandler _securityTokenHandler;
    private readonly JwtOptions _jwtOptions;

    public JwtTokenProvider(SecurityTokenHandler securityTokenHandler, IOptions<JwtOptions> jwtOptions)
    {
        _securityTokenHandler = securityTokenHandler;
        _jwtOptions = jwtOptions.Value;
    }

    public string CreateToken(IEnumerable<Claim> claims)
    {
        ArgumentNullException.ThrowIfNull(claims);

        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(_jwtOptions.ExpirationTime.GetValueOrDefault()),
            SigningCredentials = new SigningCredentials(secretKey, _jwtOptions.EncryptionAlgorithms.FirstOrDefault())
        };

        var token = _securityTokenHandler.CreateToken(tokenDescriptor);

        return _securityTokenHandler.WriteToken(token);
    }
}