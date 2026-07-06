using HedefTakip.Shared.Constants;
using HedefTakip.Shared.Models;
using HedefTakip.Shared.Services;
using IdentityService.DTOs;
using IdentityService.Services;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IAuditClient _audit;

    public AuthController(IAuthService authService, IAuditClient audit)
    {
        _authService = authService;
        _audit = audit;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        if (result is null)
            return Unauthorized(ApiResponse.Fail(Messages.Auth.InvalidCredentials, Messages.Auth.InvalidCredentialsUser, Messages.Auth.InvalidCredentialsUser));

        _audit.Send("IdentityService", "Login", "User", result.User.Id.ToString(),
            result.User.Id.ToString(), $"Email: {result.User.Email}");

        return Ok(ApiResponse<LoginResponse>.Ok(result, Messages.Auth.LoginSuccess));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshAsync(request);
        if (result is null)
            return Unauthorized(ApiResponse.Fail(Messages.Auth.InvalidRefreshToken, Messages.Auth.InvalidRefreshTokenUser, Messages.Auth.InvalidRefreshTokenUser));

        return Ok(ApiResponse<RefreshTokenResponse>.Ok(result));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;
        await _authService.LogoutAsync(request.RefreshToken);
        _audit.Send("IdentityService", "Logout", "User", userId, userId);
        return Ok(ApiResponse.OkNoData(Messages.Auth.LogoutSuccess));
    }
}
