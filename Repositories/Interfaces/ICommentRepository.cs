using BugTracker.API.Models;

namespace BugTracker.API.Repositories.Interfaces;

public interface ICommentRepository
{
    Task<IEnumerable<Comment>> GetByIssueIdAsync(int issueId);
    Task<Comment?> GetByIdAsync(int id);
    Task<Comment> CreateAsync(Comment comment);
    Task DeleteAsync(Comment comment);
}
