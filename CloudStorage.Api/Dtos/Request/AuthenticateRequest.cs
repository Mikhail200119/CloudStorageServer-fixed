using System.ComponentModel.DataAnnotations;

namespace CloudStorage.Api.Dtos.Request;

public class AuthenticateRequest
{
    [Required]
    [MinLength(1)]
    public string Email { get; set; }
    [Required]
    [MinLength(1)]
    public string Password { get; set; }
}