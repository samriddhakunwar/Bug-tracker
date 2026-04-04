namespace BugTracker.API.Models;

public enum IssueStatus
{
    Open = 0,
    InProgress = 1,
    Resolved = 2
}

public enum Priority
{
    Low = 0,
    Medium = 1,
    High = 2
}

public enum Severity
{
    Minor = 0,
    Major = 1,
    Critical = 2
}

public enum UserRole
{
    Developer = 0,
    Admin = 1
}
