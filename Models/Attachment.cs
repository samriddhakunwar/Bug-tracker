namespace BugTracker.API.Models;

public class Attachment
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public int IssueId { get; set; }
    public Issue? Issue { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }
}
