using BugTracker.API.DTOs.Users;
using BugTracker.API.Repositories.Interfaces;
using BugTracker.API.Services.Interfaces;

namespace BugTracker.API.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepo;

    public UserService(IUserRepository userRepo) => _userRepo = userRepo;

    public async Task<IEnumerable<UserResponseDto>> GetAllAsync()
    {
        var users = await _userRepo.GetAllAsync();
        return users.Select(u => new UserResponseDto
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            Role = u.Role.ToString(),
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt
        });
    }

    public async Task<UserResponseDto?> GetByIdAsync(int id)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user == null) return null;

        return new UserResponseDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role.ToString(),
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<bool> DeactivateAsync(int id)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user == null) return false;

        user.IsActive = false;
        await _userRepo.UpdateAsync(user);
        return true;
    }
}
