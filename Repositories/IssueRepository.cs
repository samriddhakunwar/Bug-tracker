using BugTracker.API.Data;
using BugTracker.API.Models;
using BugTracker.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.API.Repositories;

public class IssueRepository : IIssueRepository
{
    private readonly AppDbContext _db;

    public IssueRepository(AppDbContext db) => _db = db;

    public async Task<(IEnumerable<Issue> Issues, int TotalCount)> GetAllAsync(IssueFilter filter)
    {
        var query = _db.Issues
            .Include(i => i.CreatedByUser)
            .Include(i => i.AssignedToUser)
            .Include(i => i.Comments)
            .Include(i => i.Attachments)
            .AsQueryable();

        if (filter.Status.HasValue)
            query = query.Where(i => i.Status == filter.Status.Value);

        if (filter.Priority.HasValue)
            query = query.Where(i => i.Priority == filter.Priority.Value);

        if (filter.AssignedToUserId.HasValue)
            query = query.Where(i => i.AssignedToUserId == filter.AssignedToUserId.Value);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.ToLower();
            query = query.Where(i =>
                i.Title.ToLower().Contains(term) ||
                i.Description.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync();

        var issues = await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return (issues, totalCount);
    }

    public async Task<Issue?> GetByIdAsync(int id) =>
        await _db.Issues
            .Include(i => i.CreatedByUser)
            .Include(i => i.AssignedToUser)
            .Include(i => i.Comments).ThenInclude(c => c.User)
            .Include(i => i.Attachments)
            .Include(i => i.ActivityLogs).ThenInclude(a => a.User)
            .FirstOrDefaultAsync(i => i.Id == id);

    public async Task<Issue> CreateAsync(Issue issue)
    {
        _db.Issues.Add(issue);
        await _db.SaveChangesAsync();
        return issue;
    }

    public async Task<Issue> UpdateAsync(Issue issue)
    {
        _db.Issues.Update(issue);
        await _db.SaveChangesAsync();
        return issue;
    }

    public async Task DeleteAsync(Issue issue)
    {
        _db.Issues.Remove(issue);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id) =>
        await _db.Issues.AnyAsync(i => i.Id == id);

    public async Task<int> CountByStatusAsync(IssueStatus status) =>
        await _db.Issues.CountAsync(i => i.Status == status);
}
