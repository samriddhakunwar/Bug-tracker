using System.ComponentModel.DataAnnotations;
using BugTracker.API.Models;

namespace BugTracker.API.DTOs.Issues;

public class CreateIssueDto
{
    [Required, MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public Priority Priority { get; set; } = Priority.Medium;
    public Severity Severity { get; set; } = Severity.Minor;
    public int? AssignedToUserId { get; set; }
}

public class UpdateIssueDto
{
    [MaxLength(300)]
    public string? Title { get; set; }
    public string? Description { get; set; }
    public IssueStatus? Status { get; set; }
    public Priority? Priority { get; set; }
    public Severity? Severity { get; set; }
    public int? AssignedToUserId { get; set; }
}

public class IssueResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedByUsername { get; set; } = string.Empty;
    public int CreatedByUserId { get; set; }
    public string? AssignedToUsername { get; set; }
    public int? AssignedToUserId { get; set; }
    public int CommentCount { get; set; }
    public int AttachmentCount { get; set; }
}

public class IssueListResponseDto
{
    public List<IssueResponseDto> Issues { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
