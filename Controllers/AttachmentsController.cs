using BugTracker.API.Data;
using BugTracker.API.Helpers;
using BugTracker.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.API.Controllers;

[ApiController]
[Route("api/attachments")]
[Authorize]
public class AttachmentsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<AttachmentsController> _logger;

    public AttachmentsController(AppDbContext db, IWebHostEnvironment env, ILogger<AttachmentsController> logger)
    {
        _db = db;
        _env = env;
        _logger = logger;
    }

    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".pdf", ".txt", ".log", ".zip"];
    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

    /// <summary>Upload a file attachment to an issue</summary>
    [HttpPost("{issueId:int}")]
    public async Task<IActionResult> Upload(int issueId, IFormFile file)
    {
        var issue = await _db.Issues.FindAsync(issueId);
        if (issue == null)
            return NotFound(new { success = false, message = "Issue not found." });

        if (file == null || file.Length == 0)
            return BadRequest(new { success = false, message = "No file provided." });

        if (file.Length > MaxFileSize)
            return BadRequest(new { success = false, message = "File exceeds 10 MB limit." });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            return BadRequest(new { success = false, message = "File type not allowed." });

        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
        Directory.CreateDirectory(uploadsDir);

        var uniqueName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsDir, uniqueName);

        using (var stream = new FileStream(filePath, FileMode.Create))
            await file.CopyToAsync(stream);

        var attachment = new Attachment
        {
            IssueId = issueId,
            UserId = JwtHelper.GetUserId(User),
            FileName = file.FileName,
            FilePath = $"/uploads/{uniqueName}",
            FileSize = file.Length,
            ContentType = file.ContentType
        };

        _db.Attachments.Add(attachment);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            data = new
            {
                attachment.Id,
                attachment.FileName,
                attachment.FilePath,
                attachment.FileSize,
                attachment.ContentType,
                attachment.UploadedAt
            }
        });
    }

    /// <summary>Delete an attachment (Admin or uploader)</summary>
    [HttpDelete("{attachmentId:int}")]
    public async Task<IActionResult> Delete(int attachmentId)
    {
        var attachment = await _db.Attachments.FindAsync(attachmentId);
        if (attachment == null)
            return NotFound(new { success = false, message = "Attachment not found." });

        var userId = JwtHelper.GetUserId(User);
        var isAdmin = JwtHelper.IsAdmin(User);

        if (!isAdmin && attachment.UserId != userId)
            return Forbid();

        var physicalPath = Path.Combine(_env.WebRootPath, attachment.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (System.IO.File.Exists(physicalPath))
            System.IO.File.Delete(physicalPath);

        _db.Attachments.Remove(attachment);
        await _db.SaveChangesAsync();

        return Ok(new { success = true, message = "Attachment deleted." });
    }
}
