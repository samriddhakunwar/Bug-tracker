using BugTracker.API.Data;
using BugTracker.API.Models;
using BugTracker.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.API.Repositories;

public class ActivityLogRepository : IActivityLogRepository
{
    private readonly AppDbContext _db;

    public ActivityLogRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<ActivityLog>> GetByIssueIdAsync(int issueId) =>
        await _db.ActivityLogs
            .Include(a => a.User)
            .Where(a => a.IssueId == issueId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

    public async Task<ActivityLog> CreateAsync(ActivityLog log)
    {
        _db.ActivityLogs.Add(log);
        await _db.SaveChangesAsync();
        return log;
    }
}
