using BugTracker.API.DTOs.Auth;
using BugTracker.API.Helpers;
using BugTracker.API.Models;
using BugTracker.API.Repositories.Interfaces;
using BugTracker.API.Services.Interfaces;

namespace BugTracker.API.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly JwtHelper _jwtHelper;

    public AuthService(IUserRepository userRepo, JwtHelper jwtHelper)
    {
        _userRepo = userRepo;
        _jwtHelper = jwtHelper;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        if (await _userRepo.GetByEmailAsync(dto.Email) != null)
            throw new InvalidOperationException("Email is already registered.");

        if (await _userRepo.GetByUsernameAsync(dto.Username) != null)
            throw new InvalidOperationException("Username is already taken.");

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = UserRole.Developer
        };

        await _userRepo.CreateAsync(user);

        return new AuthResponseDto
        {
            Token = _jwtHelper.GenerateToken(user),
            Username = user.Username,
            Email = user.Email,
            Role = user.Role.ToString(),
            UserId = user.Id
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userRepo.GetByEmailAsync(dto.Email.ToLower())
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account is deactivated.");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        return new AuthResponseDto
        {
            Token = _jwtHelper.GenerateToken(user),
            Username = user.Username,
            Email = user.Email,
            Role = user.Role.ToString(),
            UserId = user.Id
        };
    }
}
