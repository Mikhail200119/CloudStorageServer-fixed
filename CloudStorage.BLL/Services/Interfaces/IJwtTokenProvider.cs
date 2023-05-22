using System.Security.Claims;

namespace CloudStorage.BLL.Services.Interfaces;

public interface IJwtTokenProvider
{
    string CreateToken(IEnumerable<Claim> claims);
}