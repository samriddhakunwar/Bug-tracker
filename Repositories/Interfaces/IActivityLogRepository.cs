using BugTracker.API.Models;

namespace BugTracker.API.Repositories.Interfaces;

public interface IActivityLogRepository
{
    Task<IEnumerable<ActivityLog>> GetByIssueIdAsync(int issueId);
    Task<ActivityLog> CreateAsync(ActivityLog log);
}
