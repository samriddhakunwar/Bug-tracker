namespace BugTracker.API.DTOs.ActivityLogs;

public class ActivityLogResponseDto
{
    public int Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public int IssueId { get; set; }
}
