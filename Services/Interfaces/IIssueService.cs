using BugTracker.API.DTOs.Issues;
using BugTracker.API.Repositories.Interfaces;

namespace BugTracker.API.Services.Interfaces;

public interface IIssueService
{
    Task<IssueListResponseDto> GetAllAsync(IssueFilter filter);
    Task<IssueResponseDto?> GetByIdAsync(int id);
    Task<IssueResponseDto> CreateAsync(CreateIssueDto dto, int createdByUserId);
    Task<IssueResponseDto?> UpdateAsync(int id, UpdateIssueDto dto, int updatedByUserId);
    Task<bool> DeleteAsync(int id);
    Task<DashboardStatsDto> GetDashboardStatsAsync();
}

public class DashboardStatsDto
{
    public int OpenCount { get; set; }
    public int InProgressCount { get; set; }
    public int ResolvedCount { get; set; }
    public int TotalCount { get; set; }
}
