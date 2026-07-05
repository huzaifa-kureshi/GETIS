using GETIS.Api.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace GETIS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;

    public AuthController(IConfiguration config)
    {
        _config = config;
    }

    // POST api/auth/login
    [HttpPost("login")]
    public ActionResult Login(LoginDto dto)
    {
        var expectedUser = _config["AdminCredentials:Username"];
        var expectedPass = _config["AdminCredentials:Password"];

        if (dto.Username == expectedUser && dto.Password == expectedPass)
        {
            var token = _config["AdminToken"];
            return Ok(new { token });
        }

        return Unauthorized(new { error = "Invalid username or password." });
    }
}
