using BugTracker.API.DTOs.Comments;
using BugTracker.API.Models;
using BugTracker.API.Repositories.Interfaces;
using BugTracker.API.Services.Interfaces;

namespace BugTracker.API.Services;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepo;
    private readonly IActivityLogRepository _logRepo;

    public CommentService(ICommentRepository commentRepo, IActivityLogRepository logRepo)
    {
        _commentRepo = commentRepo;
        _logRepo = logRepo;
    }

    public async Task<IEnumerable<CommentResponseDto>> GetByIssueIdAsync(int issueId)
    {
        var comments = await _commentRepo.GetByIssueIdAsync(issueId);
        return comments.Select(c => new CommentResponseDto
        {
            Id = c.Id,
            Content = c.Content,
            CreatedAt = c.CreatedAt,
            UserId = c.UserId,
            Username = c.User?.Username ?? string.Empty,
            IssueId = c.IssueId
        });
    }

    public async Task<CommentResponseDto> CreateAsync(int issueId, CreateCommentDto dto, int userId)
    {
        var comment = new Comment
        {
            IssueId = issueId,
            UserId = userId,
            Content = dto.Content
        };

        await _commentRepo.CreateAsync(comment);

        await _logRepo.CreateAsync(new ActivityLog
        {
            IssueId = issueId,
            UserId = userId,
            Action = "Commented",
            NewValue = dto.Content.Length > 100 ? dto.Content[..100] + "..." : dto.Content
        });

        var created = await _commentRepo.GetByIdAsync(comment.Id);
        return new CommentResponseDto
        {
            Id = created!.Id,
            Content = created.Content,
            CreatedAt = created.CreatedAt,
            UserId = created.UserId,
            Username = created.User?.Username ?? string.Empty,
            IssueId = created.IssueId
        };
    }

    public async Task<bool> DeleteAsync(int commentId, int requestingUserId, bool isAdmin)
    {
        var comment = await _commentRepo.GetByIdAsync(commentId);
        if (comment == null) return false;

        if (!isAdmin && comment.UserId != requestingUserId)
            throw new UnauthorizedAccessException("You can only delete your own comments.");

        await _commentRepo.DeleteAsync(comment);
        return true;
    }
}
