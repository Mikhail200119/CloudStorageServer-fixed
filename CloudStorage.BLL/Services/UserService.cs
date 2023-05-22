using CloudStorage.BLL.Models;
using CloudStorage.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace CloudStorage.BLL.Services;

public sealed class UserService : IUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public User Current => BuildUserModel(_httpContextAccessor.HttpContext.User.Claims);

    private static User BuildUserModel(IEnumerable<Claim> claims) =>
        new()
        {
            Email = claims.FirstOrDefault(claim => claim.Type.EndsWith("emailaddress"))?.Value ?? throw new ApplicationException()
        };
}