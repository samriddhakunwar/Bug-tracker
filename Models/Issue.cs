namespace BugTracker.API.Models;

public class Issue
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IssueStatus Status { get; set; } = IssueStatus.Open;
    public Priority Priority { get; set; } = Priority.Medium;
    public Severity Severity { get; set; } = Severity.Minor;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // FK - Creator
    public int CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }

    // FK - Assignee (nullable)
    public int? AssignedToUserId { get; set; }
    public User? AssignedToUser { get; set; }

    // Navigation
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    public ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
}
