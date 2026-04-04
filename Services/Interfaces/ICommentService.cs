using BugTracker.API.DTOs.Comments;

namespace BugTracker.API.Services.Interfaces;

public interface ICommentService
{
    Task<IEnumerable<CommentResponseDto>> GetByIssueIdAsync(int issueId);
    Task<CommentResponseDto> CreateAsync(int issueId, CreateCommentDto dto, int userId);
    Task<bool> DeleteAsync(int commentId, int requestingUserId, bool isAdmin);
}
