using BugTracker.API.Data;
using BugTracker.API.Models;
using BugTracker.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.API.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly AppDbContext _db;

    public CommentRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<Comment>> GetByIssueIdAsync(int issueId) =>
        await _db.Comments
            .Include(c => c.User)
            .Where(c => c.IssueId == issueId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

    public async Task<Comment?> GetByIdAsync(int id) =>
        await _db.Comments.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == id);

    public async Task<Comment> CreateAsync(Comment comment)
    {
        _db.Comments.Add(comment);
        await _db.SaveChangesAsync();
        return comment;
    }

    public async Task DeleteAsync(Comment comment)
    {
        _db.Comments.Remove(comment);
        await _db.SaveChangesAsync();
    }
}
