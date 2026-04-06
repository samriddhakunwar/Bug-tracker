using BugTracker.API.Models;
using BCrypt.Net;

namespace BugTracker.API.Data;

public static class DbSeeder
{
    public static void SeedDatabase(AppDbContext context)
    {
        // Prevent re-seeding
        if (context.Users.Any())
            return;

        // Create demo users
        var admin = new User
        {
            Username = "admin",
            Email = "admin@bugtracker.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Role = UserRole.Admin,
            CreatedAt = DateTime.UtcNow
        };

        var dev1 = new User
        {
            Username = "john_dev",
            Email = "john@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Dev@123"),
            Role = UserRole.Developer,
            CreatedAt = DateTime.UtcNow
        };

        var dev2 = new User
        {
            Username = "jane_dev",
            Email = "jane@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Dev@123"),
            Role = UserRole.Developer,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.AddRange(admin, dev1, dev2);
        context.SaveChanges();

        // Fetch created user IDs
        var adminId = admin.Id;
        var dev1Id = dev1.Id;
        var dev2Id = dev2.Id;

        // Create demo issues
        var issues = new List<Issue>
        {
            new Issue
            {
                Title = "Login page not responding on mobile",
                Description = "Users report that the login page hangs when accessed on Safari iOS 15+. The issue appears to be related to JavaScript promise handling.",
                CreatedByUserId = adminId,
                AssignedToUserId = dev1Id,
                Status = IssueStatus.Open,
                Priority = IssuePriority.High,
                Severity = IssueSeverity.Critical,
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new Issue
            {
                Title = "Database connection timeout on prod",
                Description = "Intermittent connection timeouts observed in production environment. May be related to connection pooling configuration or network latency.",
                CreatedByUserId = adminId,
                AssignedToUserId = dev2Id,
                Status = IssueStatus.InProgress,
                Priority = IssuePriority.High,
                Severity = IssueSeverity.Critical,
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new Issue
            {
                Title = "Add export to PDF feature",
                Description = "Users request ability to export issue details and comments to PDF format for documentation and sharing purposes.",
                CreatedByUserId = dev1Id,
                AssignedToUserId = null,
                Status = IssueStatus.Open,
                Priority = IssuePriority.Medium,
                Severity = IssueSeverity.Minor,
                CreatedAt = DateTime.UtcNow.AddDays(-7)
            },
            new Issue
            {
                Title = "Performance optimization needed",
                Description = "Page load time for issues list is taking 3-4 seconds. Need to optimize queries and implement pagination.",
                CreatedByUserId = dev2Id,
                AssignedToUserId = dev1Id,
                Status = IssueStatus.InProgress,
                Priority = IssuePriority.Medium,
                Severity = IssueSeverity.Major,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new Issue
            {
                Title = "Fix typo in dashboard",
                Description = "Minor typo in the dashboard header - says 'Isues' instead of 'Issues'.",
                CreatedByUserId = adminId,
                AssignedToUserId = dev2Id,
                Status = IssueStatus.Resolved,
                Priority = IssuePriority.Low,
                Severity = IssueSeverity.Minor,
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            }
        };

        context.Issues.AddRange(issues);
        context.SaveChanges();

        // Create demo comments
        var issueId1 = issues[0].Id;
        var issueId2 = issues[1].Id;
        var issueId4 = issues[3].Id;

        var comments = new List<Comment>
        {
            new Comment
            {
                IssueId = issueId1,
                UserId = dev1Id,
                Text = "I've reproduced the issue on iPhone 12 with iOS 15.2. The page freezes when trying to submit the login form.",
                CreatedAt = DateTime.UtcNow.AddDays(-4)
            },
            new Comment
            {
                IssueId = issueId1,
                UserId = adminId,
                Text = "Likely a JavaScript async/await issue. Let's trace the network requests in Safari DevTools.",
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new Comment
            {
                IssueId = issueId2,
                UserId = dev2Id,
                Text = "Created CloudWatch alarms to monitor connection pool utilization. Will investigate further tomorrow.",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new Comment
            {
                IssueId = issueId4,
                UserId = dev1Id,
                Text = "Added pagination with 20 items per page. Load time now under 500ms. PR #234 ready for review.",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            }
        };

        context.Comments.AddRange(comments);
        context.SaveChanges();

        // Create demo activity logs
        var activityLogs = new List<ActivityLog>
        {
            new ActivityLog
            {
                IssueId = issueId1,
                UserId = adminId,
                Action = "created_issue",
                Details = "Issue created with priority HIGH and severity CRITICAL",
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new ActivityLog
            {
                IssueId = issueId1,
                UserId = adminId,
                Action = "assigned_issue",
                Details = "Assigned to john_dev",
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new ActivityLog
            {
                IssueId = issueId2,
                UserId = adminId,
                Action = "status_changed",
                Details = "Status changed from Open to In Progress",
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new ActivityLog
            {
                IssueId = issueId4,
                UserId = dev1Id,
                Action = "status_changed",
                Details = "Status changed from Open to In Progress",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            }
        };

        context.ActivityLogs.AddRange(activityLogs);
        context.SaveChanges();
    }
}
