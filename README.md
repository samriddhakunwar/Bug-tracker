# BugTracker API - Complete Issue Tracking System

A production-ready bug tracking and issue management system built with ASP.NET Core, Entity Framework Core, MySQL, and a responsive Bootstrap frontend.

## 🚀 Features

### Core Functionality

- **User Management**: Registration, login, and role-based access control
- **Issue Tracking**: Create, edit, delete, and assign issues with full lifecycle management
- **Comments System**: Add comments and discussion threads to issues
- **File Attachments**: Upload and manage attachments (logs, screenshots, etc.)
- **Activity Logging**: Track all changes and modifications to issues
- **Search & Filtering**: Filter issues by status, priority, severity, and assigned user
- **Responsive UI**: Clean, minimal Bootstrap-based dashboard

### Technical Features

- **JWT Authentication**: Secure token-based authentication
- **Role-Based Authorization**: Admin and Developer roles
- **Global Error Handling**: Centralized exception handling middleware
- **API Documentation**: Swagger/OpenAPI integration
- **Clean Architecture**: Controller → Service → Repository pattern
- **Entity Framework Core**: Code-first database migrations

## 📋 Tech Stack

| Component             | Technology                         |
| --------------------- | ---------------------------------- |
| **Backend**           | ASP.NET Core 8.0 (C#)              |
| **Database**          | MySQL                              |
| **ORM**               | Entity Framework Core 8.0          |
| **Authentication**    | JWT (JSON Web Tokens)              |
| **Frontend**          | HTML5, CSS3, Bootstrap, JavaScript |
| **API Documentation** | Swagger/OpenAPI                    |

## 🔧 Prerequisites

- **.NET SDK** 8.0 or later
- **MySQL Server** 5.7 or later
- **SQL Client**: MySQL Workbench or command-line tool
- **Node.js** (optional, for frontend tooling)

## 📦 Installation & Setup

### 1. Clone Repository

```bash
git clone <repository-url>
cd BugTracker.API
```

### 2. Database Configuration

Update the connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=bugtracker_db;User=root;Password=YOUR_PASSWORD_HERE;"
  }
}
```

Replace the values with your MySQL credentials.

### 3. Create Database

```bash
# Ensure MySQL is running
mysql -u root -p -e "CREATE DATABASE bugtracker_db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"
```

### 4. Restore Dependencies

```bash
dotnet restore
```

### 5. Apply Database Migrations

```bash
# Option A: Using dotnet CLI (EF Tools required)
dotnet tool install --global dotnet-ef
dotnet ef database update

# Option B: Automatic migration on application start (default)
# Just run the application - migrations apply automatically
```

### 6. Run Application

```bash
dotnet run
```

The application will be available at:

- **API**: `https://localhost:7123`
- **Frontend**: `https://localhost:7123`
- **Swagger UI**: `https://localhost:7123/swagger`

## 🔐 Authentication

### Default Admin Account

```
Email: admin@bugtracker.com
Password: Admin@123
```

**⚠️ Important**: Change the default credentials in production!

### JWT Token

- **Token Expiry**: 24 hours (configurable in `appsettings.json`)
- **Secret Key**: Must be at least 32 characters in production

```json
"JwtSettings": {
  "SecretKey": "BugTracker_SuperSecretKey_ChangeInProduction_MinLength32Chars!",
  "Issuer": "BugTrackerAPI",
  "Audience": "BugTrackerClient",
  "ExpiryHours": "24"
}
```

## 📚 API Endpoints

### Authentication

- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login and get JWT token

### Issues

- `GET /api/issues` - List all issues with pagination
- `POST /api/issues` - Create new issue
- `GET /api/issues/{id}` - Get issue details
- `PUT /api/issues/{id}` - Update issue
- `DELETE /api/issues/{id}` - Delete issue

### Comments

- `POST /api/comments` - Add comment to issue
- `GET /api/comments/{issueId}` - Get issue comments
- `DELETE /api/comments/{id}` - Delete comment

### Attachments

- `POST /api/attachments/{issueId}` - Upload file attachment
- `DELETE /api/attachments/{id}` - Delete attachment

### Activity Logs

- `GET /api/activitylogs/{issueId}` - Get issue activity history

### Users

- `GET /api/users` - List all users (Admin only)
- `GET /api/users/{id}` - Get user profile

## 📁 Project Structure

```
BugTracker.API/
├── Controllers/              # API endpoints
│   ├── AuthController.cs
│   ├── IssuesController.cs
│   ├── CommentsController.cs
│   ├── AttachmentsController.cs
│   ├── ActivityLogsController.cs
│   └── UsersController.cs
├── Services/                 # Business logic layer
│   ├── AuthService.cs
│   ├── IssueService.cs
│   ├── CommentService.cs
│   └── UserService.cs
├── Repositories/             # Data access layer
│   ├── IssueRepository.cs
│   ├── CommentRepository.cs
│   ├── UserRepository.cs
│   └── ActivityLogRepository.cs
├── Models/                   # Domain entities
│   ├── User.cs
│   ├── Issue.cs
│   ├── Comment.cs
│   ├── Attachment.cs
│   └── ActivityLog.cs
├── DTOs/                     # Data Transfer Objects
│   ├── Auth/
│   ├── Issues/
│   ├── Comments/
│   └── Users/
├── Data/                     # Database context
│   └── AppDbContext.cs
├── Helpers/                  # Utility classes
│   └── JwtHelper.cs
├── Middleware/               # Custom middleware
│   └── ExceptionHandlingMiddleware.cs
├── wwwroot/                  # Frontend (static files)
│   ├── index.html
│   ├── dashboard.html
│   ├── issues.html
│   ├── issue-detail.html
│   ├── users.html
│   ├── login.html
│   ├── register.html
│   ├── css/
│   └── js/
├── Program.cs                # Application entry point
├── appsettings.json          # Configuration
└── BugTracker.API.csproj     # Project file
```

## 🗄️ Database Schema

### Users Table

```sql
CREATE TABLE Users (
  Id INT PRIMARY KEY AUTO_INCREMENT,
  Username VARCHAR(100) UNIQUE NOT NULL,
  Email VARCHAR(200) UNIQUE NOT NULL,
  PasswordHash VARCHAR(255) NOT NULL,
  Role ENUM('Admin', 'Developer') DEFAULT 'Developer',
  CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

### Issues Table

```sql
CREATE TABLE Issues (
  Id INT PRIMARY KEY AUTO_INCREMENT,
  CreatedByUserId INT NOT NULL,
  AssignedToUserId INT,
  Title VARCHAR(300) NOT NULL,
  Description TEXT,
  Status ENUM('Open', 'In Progress', 'Resolved') DEFAULT 'Open',
  Priority ENUM('Low', 'Medium', 'High') DEFAULT 'Medium',
  Severity ENUM('Minor', 'Major', 'Critical') DEFAULT 'Minor',
  CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id),
  FOREIGN KEY (AssignedToUserId) REFERENCES Users(Id)
);
```

### Full schema is auto-generated by EF Core migrations

## 🌐 Frontend Pages

| Page         | URL                       | Description                    |
| ------------ | ------------------------- | ------------------------------ |
| Login        | `/login.html`             | User authentication            |
| Register     | `/register.html`          | New user registration          |
| Dashboard    | `/dashboard.html`         | Main dashboard with statistics |
| Issues List  | `/issues.html`            | View and manage all issues     |
| Issue Detail | `/issue-detail.html?id=X` | View/edit single issue         |
| Users        | `/users.html`             | User management (Admin)        |

## 🧪 Testing the API

### Using cURL

```bash
# Register
curl -X POST https://localhost:7123/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"testuser","email":"test@example.com","password":"Password@123"}'

# Login
curl -X POST https://localhost:7123/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Password@123"}'

# Create Issue (with token)
curl -X POST https://localhost:7123/api/issues \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -H "Content-Type: application/json" \
  -d '{
    "title":"App crashes on startup",
    "description":"App crashes when launched on Android 13",
    "priority":"High",
    "severity":"Critical"
  }'
```

### Using Swagger UI

Navigate to `https://localhost:7123/swagger` and use the interactive interface to test all endpoints.

## 📝 File Upload Configuration

- **Max File Size**: 10 MB
- **Allowed Types**: `.jpg`, `.jpeg`, `.png`, `.gif`, `.pdf`, `.txt`, `.log`, `.zip`
- **Storage**: `wwwroot/uploads` (created automatically)
- **URL Prefix**: `/uploads/`

## 🔧 Configuration Files

### appsettings.json

Main configuration file. Key settings:

- Database connection string
- JWT settings (secret key, expiry, issuer, audience)
- Logging level
- CORS policy

### appsettings.Production.json

Production-specific settings (if exists, overrides appsettings.json)

## 🚀 Deployment

### Prerequisites

- Windows Server or Linux with .NET 8.0+ runtime
- MySQL server accessible from deployment machine
- HTTPS certificate for production

### IIS Deployment (Windows)

```bash
# Publish release build
dotnet publish -c Release -o ./publish

# Deploy 'publish' folder to IIS application folder
# Create App Pool with .NET CLR version: No Managed Code
# Bind application to port 443 (HTTPS)
```

### Docker Deployment

```bash
# Build image
docker build -t bugtracker-api .

# Run container
docker run -p 7123:8080 \
  -e "ConnectionStrings__DefaultConnection=Server=db;Port=3306;..." \
  bugtracker-api
```

## 🛠️ Development & Debugging

### Enable Debug Logging

Update `appsettings.json`:

```json
"Logging": {
  "LogLevel": {
    "Default": "Debug",
    "Microsoft.AspNetCore": "Information"
  }
}
```

### Database Migrations (Development)

```bash
# Create new migration
dotnet ef migrations add MigrationName

# Apply migrations
dotnet ef database update

# Remove last migration
dotnet ef migrations remove
```

## ⚠️ Security Considerations

1. **Change JWT Secret**: Update `JwtSettings.SecretKey` in production
2. **Disable Swagger**: Disable in production by checking `app.Environment.IsProduction()`
3. **CORS Policy**: Configure actual origin URLs instead of `AllowAnyOrigin()`
4. **Password Policy**: Enforce strong password requirements
5. **HTTPS Only**: Always use HTTPS in production
6. **Input Validation**: All user inputs are validated server-side

## 🐛 Troubleshooting

### Database Connection Error

```
"Unable to connect to MySQL server"
```

- Verify MySQL is running
- Check connection string in `appsettings.json`
- Verify database exists: `SHOW DATABASES;`

### JWT Authentication Failure

```
"401 Unauthorized"
```

- Ensure token is included in Authorization header: `Bearer {token}`
- Verify token hasn't expired
- Check JWT secret key matches configuration

### File Upload Error

```
"File exceeds 10 MB limit"
```

- Check file size is under 10 MB
- Verify file type is in allowed list
- Ensure `wwwroot/uploads` directory exists and is writable

## 📞 Support & Contributions

For issues, questions, or suggestions, please create an issue in the repository.

## 📄 License

This project is provided as-is for educational and commercial use.

---

**Last Updated**: April 2026
**Version**: 1.0.0
**Status**: Production Ready
