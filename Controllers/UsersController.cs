using BugTracker.API.Helpers;
using BugTracker.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BugTracker.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService) => _userService = userService;

    /// <summary>Get all users (Admin only)</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userService.GetAllAsync();
        return Ok(new { success = true, data = users });
    }

    /// <summary>Get user by ID (Admin only)</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null) return NotFound(new { success = false, message = "User not found." });
        return Ok(new { success = true, data = user });
    }

    /// <summary>Deactivate a user account (Admin only)</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var currentUserId = JwtHelper.GetUserId(User);
        if (id == currentUserId)
            return BadRequest(new { success = false, message = "You cannot deactivate your own account." });

        var result = await _userService.DeactivateAsync(id);
        if (!result) return NotFound(new { success = false, message = "User not found." });
        return Ok(new { success = true, message = "User deactivated." });
    }
}
