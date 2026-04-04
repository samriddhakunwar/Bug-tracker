using BugTracker.API.Models;

namespace BugTracker.API.Repositories.Interfaces;

public class IssueFilter
{
    public IssueStatus? Status { get; set; }
    public Priority? Priority { get; set; }
    public int? AssignedToUserId { get; set; }
    public string? SearchTerm { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public interface IIssueRepository
{
    Task<(IEnumerable<Issue> Issues, int TotalCount)> GetAllAsync(IssueFilter filter);
    Task<Issue?> GetByIdAsync(int id);
    Task<Issue> CreateAsync(Issue issue);
    Task<Issue> UpdateAsync(Issue issue);
    Task DeleteAsync(Issue issue);
    Task<bool> ExistsAsync(int id);
    Task<int> CountByStatusAsync(IssueStatus status);
}
