using BugTracker.API.DTOs.Issues;
using BugTracker.API.Models;
using BugTracker.API.Repositories.Interfaces;
using BugTracker.API.Services.Interfaces;

namespace BugTracker.API.Services;

public class IssueService : IIssueService
{
    private readonly IIssueRepository _issueRepo;
    private readonly IActivityLogRepository _logRepo;

    public IssueService(IIssueRepository issueRepo, IActivityLogRepository logRepo)
    {
        _issueRepo = issueRepo;
        _logRepo = logRepo;
    }

    public async Task<IssueListResponseDto> GetAllAsync(IssueFilter filter)
    {
        var (issues, totalCount) = await _issueRepo.GetAllAsync(filter);
        var totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize);

        return new IssueListResponseDto
        {
            Issues = issues.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalPages = totalPages
        };
    }

    public async Task<IssueResponseDto?> GetByIdAsync(int id)
    {
        var issue = await _issueRepo.GetByIdAsync(id);
        return issue == null ? null : MapToDto(issue);
    }

    public async Task<IssueResponseDto> CreateAsync(CreateIssueDto dto, int createdByUserId)
    {
        var issue = new Issue
        {
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            Severity = dto.Severity,
            AssignedToUserId = dto.AssignedToUserId,
            CreatedByUserId = createdByUserId,
            Status = IssueStatus.Open
        };

        await _issueRepo.CreateAsync(issue);

        await _logRepo.CreateAsync(new ActivityLog
        {
            IssueId = issue.Id,
            UserId = createdByUserId,
            Action = "Created",
            NewValue = $"Issue '{issue.Title}' created"
        });

        var created = await _issueRepo.GetByIdAsync(issue.Id);
        return MapToDto(created!);
    }

    public async Task<IssueResponseDto?> UpdateAsync(int id, UpdateIssueDto dto, int updatedByUserId)
    {
        var issue = await _issueRepo.GetByIdAsync(id);
        if (issue == null) return null;

        var changes = new List<string>();

        if (dto.Title != null && dto.Title != issue.Title)
        {
            changes.Add($"Title: '{issue.Title}' → '{dto.Title}'");
            issue.Title = dto.Title;
        }
        if (dto.Description != null && dto.Description != issue.Description)
        {
            issue.Description = dto.Description;
        }
        if (dto.Status.HasValue && dto.Status.Value != issue.Status)
        {
            changes.Add($"Status: '{issue.Status}' → '{dto.Status.Value}'");
            issue.Status = dto.Status.Value;
        }
        if (dto.Priority.HasValue && dto.Priority.Value != issue.Priority)
        {
            changes.Add($"Priority: '{issue.Priority}' → '{dto.Priority.Value}'");
            issue.Priority = dto.Priority.Value;
        }
        if (dto.Severity.HasValue && dto.Severity.Value != issue.Severity)
        {
            changes.Add($"Severity: '{issue.Severity}' → '{dto.Severity.Value}'");
            issue.Severity = dto.Severity.Value;
        }
        if (dto.AssignedToUserId != issue.AssignedToUserId)
        {
            changes.Add($"Assignee changed");
            issue.AssignedToUserId = dto.AssignedToUserId;
        }

        issue.UpdatedAt = DateTime.UtcNow;
        await _issueRepo.UpdateAsync(issue);

        if (changes.Any())
        {
            await _logRepo.CreateAsync(new ActivityLog
            {
                IssueId = issue.Id,
                UserId = updatedByUserId,
                Action = "Updated",
                NewValue = string.Join("; ", changes)
            });
        }

        var updated = await _issueRepo.GetByIdAsync(id);
        return MapToDto(updated!);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var issue = await _issueRepo.GetByIdAsync(id);
        if (issue == null) return false;

        await _issueRepo.DeleteAsync(issue);
        return true;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        var open = await _issueRepo.CountByStatusAsync(IssueStatus.Open);
        var inProgress = await _issueRepo.CountByStatusAsync(IssueStatus.InProgress);
        var resolved = await _issueRepo.CountByStatusAsync(IssueStatus.Resolved);

        return new DashboardStatsDto
        {
            OpenCount = open,
            InProgressCount = inProgress,
            ResolvedCount = resolved,
            TotalCount = open + inProgress + resolved
        };
    }

    private static IssueResponseDto MapToDto(Issue issue) => new()
    {
        Id = issue.Id,
        Title = issue.Title,
        Description = issue.Description,
        Status = issue.Status.ToString(),
        Priority = issue.Priority.ToString(),
        Severity = issue.Severity.ToString(),
        CreatedAt = issue.CreatedAt,
        UpdatedAt = issue.UpdatedAt,
        CreatedByUserId = issue.CreatedByUserId,
        CreatedByUsername = issue.CreatedByUser?.Username ?? string.Empty,
        AssignedToUserId = issue.AssignedToUserId,
        AssignedToUsername = issue.AssignedToUser?.Username,
        CommentCount = issue.Comments?.Count ?? 0,
        AttachmentCount = issue.Attachments?.Count ?? 0
    };
}
