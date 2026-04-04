using BugTracker.API.Data;
using BugTracker.API.DTOs.ActivityLogs;
using BugTracker.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.API.Controllers;

[ApiController]
[Route("api/activity-logs")]
[Authorize]
public class ActivityLogsController : ControllerBase
{
    private readonly IActivityLogRepository _logRepo;

    public ActivityLogsController(IActivityLogRepository logRepo) => _logRepo = logRepo;

    /// <summary>Get activity log for a specific issue</summary>
    [HttpGet("{issueId:int}")]
    public async Task<IActionResult> GetByIssueId(int issueId)
    {
        var logs = await _logRepo.GetByIssueIdAsync(issueId);
        var result = logs.Select(l => new ActivityLogResponseDto
        {
            Id = l.Id,
            Action = l.Action,
            OldValue = l.OldValue,
            NewValue = l.NewValue,
            CreatedAt = l.CreatedAt,
            UserId = l.UserId,
            Username = l.User?.Username ?? string.Empty,
            IssueId = l.IssueId
        });
        return Ok(new { success = true, data = result });
    }
}
