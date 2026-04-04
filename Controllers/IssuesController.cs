using BugTracker.API.DTOs.Issues;
using BugTracker.API.Helpers;
using BugTracker.API.Repositories.Interfaces;
using BugTracker.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BugTracker.API.Controllers;

[ApiController]
[Route("api/issues")]
[Authorize]
public class IssuesController : ControllerBase
{
    private readonly IIssueService _issueService;

    public IssuesController(IIssueService issueService) => _issueService = issueService;

    /// <summary>Get paginated, filtered list of issues</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status,
        [FromQuery] string? priority,
        [FromQuery] int? assignedToUserId,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var filter = new IssueFilter
        {
            SearchTerm = search,
            AssignedToUserId = assignedToUserId,
            Page = Math.Max(1, page),
            PageSize = Math.Clamp(pageSize, 1, 50)
        };

        if (Enum.TryParse<Models.IssueStatus>(status, true, out var s)) filter.Status = s;
        if (Enum.TryParse<Models.Priority>(priority, true, out var p)) filter.Priority = p;

        var result = await _issueService.GetAllAsync(filter);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Get dashboard statistics</summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _issueService.GetDashboardStatsAsync();
        return Ok(new { success = true, data = stats });
    }

    /// <summary>Get a single issue by ID</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var issue = await _issueService.GetByIdAsync(id);
        if (issue == null) return NotFound(new { success = false, message = "Issue not found." });
        return Ok(new { success = true, data = issue });
    }

    /// <summary>Create a new issue</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateIssueDto dto)
    {
        var userId = JwtHelper.GetUserId(User);
        var result = await _issueService.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, new { success = true, data = result });
    }

    /// <summary>Update an existing issue</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateIssueDto dto)
    {
        var userId = JwtHelper.GetUserId(User);
        var result = await _issueService.UpdateAsync(id, dto, userId);
        if (result == null) return NotFound(new { success = false, message = "Issue not found." });
        return Ok(new { success = true, data = result });
    }

    /// <summary>Delete an issue (Admin only)</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _issueService.DeleteAsync(id);
        if (!deleted) return NotFound(new { success = false, message = "Issue not found." });
        return Ok(new { success = true, message = "Issue deleted." });
    }
}
