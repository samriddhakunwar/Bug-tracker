using System.ComponentModel.DataAnnotations;

namespace BugTracker.API.DTOs.Comments;

public class CreateCommentDto
{
    [Required, MaxLength(2000)]
    public string Content { get; set; } = string.Empty;
}

public class CommentResponseDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public int IssueId { get; set; }
}
