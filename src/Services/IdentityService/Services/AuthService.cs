using IdentityService.Data;
using IdentityService.DTOs;
using IdentityService.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<RefreshTokenResponse?> RefreshAsync(RefreshTokenRequest request);
    Task LogoutAsync(string refreshToken);
}

public class AuthService : IAuthService
{
    private readonly IdentityDbContext _db;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _config;

    public AuthService(IdentityDbContext db, ITokenService tokenService, IConfiguration config)
    {
        _db = db;
        _tokenService = tokenService;
        _config = config;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _db.Users
            .Include(u => u.Department)
            .Include(u => u.Position)
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddDays(int.Parse(_config["Jwt:RefreshTokenExpirationDays"]!));

        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = expiresAt
        });

        await _db.SaveChangesAsync();

        return new LoginResponse(
            accessToken,
            refreshToken,
            DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:AccessTokenExpirationMinutes"]!)),
            MapToDto(user)
        );
    }

    public async Task<RefreshTokenResponse?> RefreshAsync(RefreshTokenRequest request)
    {
        var storedToken = await _db.RefreshTokens
            .Include(rt => rt.User)
            .ThenInclude(u => u.Department)
            .FirstOrDefaultAsync(rt =>
                rt.Token == request.RefreshToken &&
                !rt.IsRevoked &&
                rt.ExpiresAt > DateTime.UtcNow);

        if (storedToken is null || !storedToken.User.IsActive)
            return null;

        // Eski token'ı iptal et, yeni token üret (rotation)
        storedToken.IsRevoked = true;

        var newRefreshToken = _tokenService.GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddDays(int.Parse(_config["Jwt:RefreshTokenExpirationDays"]!));

        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = storedToken.UserId,
            Token = newRefreshToken,
            ExpiresAt = expiresAt
        });

        await _db.SaveChangesAsync();

        var accessToken = _tokenService.GenerateAccessToken(storedToken.User);

        return new RefreshTokenResponse(
            accessToken,
            newRefreshToken,
            DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:AccessTokenExpirationMinutes"]!))
        );
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var token = await _db.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (token is not null)
        {
            token.IsRevoked = true;
            await _db.SaveChangesAsync();
        }
    }

    private static UserDto MapToDto(User user) => new(
        user.Id,
        user.FirstName,
        user.LastName,
        user.Email,
        user.Role,
        user.DepartmentId,
        user.Department?.Name,
        user.PositionId,
        user.Position?.Name,
        user.IsActive
    );
}
