using BugTracker.API.DTOs.Comments;
using BugTracker.API.Helpers;
using BugTracker.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BugTracker.API.Controllers;

[ApiController]
[Route("api/issues/{issueId:int}/comments")]
[Authorize]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService) => _commentService = commentService;

    /// <summary>Get all comments for an issue</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(int issueId)
    {
        var comments = await _commentService.GetByIssueIdAsync(issueId);
        return Ok(new { success = true, data = comments });
    }

    /// <summary>Add a comment to an issue</summary>
    [HttpPost]
    public async Task<IActionResult> Create(int issueId, [FromBody] CreateCommentDto dto)
    {
        var userId = JwtHelper.GetUserId(User);
        var result = await _commentService.CreateAsync(issueId, dto, userId);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Delete a comment (owner or Admin)</summary>
    [HttpDelete("{commentId:int}")]
    public async Task<IActionResult> Delete(int issueId, int commentId)
    {
        var userId = JwtHelper.GetUserId(User);
        var isAdmin = JwtHelper.IsAdmin(User);
        var deleted = await _commentService.DeleteAsync(commentId, userId, isAdmin);
        if (!deleted) return NotFound(new { success = false, message = "Comment not found." });
        return Ok(new { success = true, message = "Comment deleted." });
    }
}
