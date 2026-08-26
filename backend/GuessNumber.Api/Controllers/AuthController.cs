using GuessNumber.Application.DTOs;
using GuessNumber.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GuessNumber.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
        => Ok(await _auth.RegisterAsync(dto));

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
        => Ok(await _auth.LoginAsync(dto));

    // Logout is client-side for JWT: the client just deletes the token.
    // This endpoint exists so the frontend can call a consistent API.
    [HttpPost("logout")]
    public IActionResult Logout() => Ok(new { message = "Logged out." });
}