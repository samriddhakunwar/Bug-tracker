using BugTracker.API.DTOs.Users;

namespace BugTracker.API.Services.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserResponseDto>> GetAllAsync();
    Task<UserResponseDto?> GetByIdAsync(int id);
    Task<bool> DeactivateAsync(int id);
}
