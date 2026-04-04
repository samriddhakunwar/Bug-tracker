namespace BugTracker.API.Models;

public class ActivityLog
{
    public int Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int IssueId { get; set; }
    public Issue? Issue { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }
}
