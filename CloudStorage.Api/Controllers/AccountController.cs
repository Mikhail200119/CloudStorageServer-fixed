using System.Security.Claims;
using CloudStorage.Api.Dtos.Request;
using CloudStorage.Api.Dtos.Response;
using CloudStorage.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace CloudStorage.Api.Controllers;

[ApiController]
[Route("api/account/")]
[AllowAnonymous]
public class AccountController : ControllerBase
{
    private readonly IJwtTokenProvider _jwtTokenProvider;
    private readonly IUsersManager _usersManager;

    public AccountController(IJwtTokenProvider jwtTokenProvider, IUsersManager usersManager)
    {
        _jwtTokenProvider = jwtTokenProvider;
        _usersManager = usersManager;
    }

    [HttpPost("authenticate")]
    public async Task<ActionResult<AuthenticateResponse>> Authenticate(AuthenticateRequest authenticateRequest)
    {
        await _usersManager.Login(authenticateRequest.Email, authenticateRequest.Password);
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, authenticateRequest.Email)
        };

        var token = _jwtTokenProvider.CreateToken(claims);

        return Ok(new AuthenticateResponse
        {
            Email = authenticateRequest.Email,
            Token = token
        });
    }

    [EnableCors("_myAllowSpecificOrigins")]
    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest registerRequest)
    {
        await _usersManager.Register(registerRequest.Email, registerRequest.Password);

        return Ok(new RegisterResponse
        {
            Email = registerRequest.Email
        });
    }
}