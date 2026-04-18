using BiblioRate.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BiblioRate.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    /// <summary>
    /// Register endpoint iskeleti (gecici).
    /// </summary>
    [HttpPost("register")]
    public IActionResult Register([FromBody] UserRegisterDto request)
    {
        return Ok(new
        {
            message = "Register endpoint hazir. Gercek dogrulama daha sonra eklenecek.",
            user = request
        });
    }

    /// <summary>
    /// Login endpoint iskeleti (gecici).
    /// </summary>
    [HttpPost("login")]
    public IActionResult Login([FromBody] UserLoginDto request)
    {
        return Ok(new
        {
            message = "Login endpoint hazir. Gercek dogrulama daha sonra eklenecek.",
            login = request
        });
    }
}
