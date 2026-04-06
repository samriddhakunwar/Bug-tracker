# BugTracker API - Complete Endpoint Documentation

## Base URL

```
https://localhost:7123/api
```

For production: Replace with your domain

All responses are in JSON format.

---

## Authentication

### Authentication Header

Include JWT token in all protected endpoints:

```
Authorization: Bearer {token}
```

### Login & Get Token

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "admin@bugtracker.com",
  "password": "Admin@123"
}

Response (200 OK):
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "user": {
      "id": 1,
      "username": "admin",
      "email": "admin@bugtracker.com",
      "role": "Admin"
    }
  }
}
```

---

## Authorization Endpoints

### `POST /auth/register`

Register a new user.

**Request:**

```json
{
  "username": "john_dev",
  "email": "john@example.com",
  "password": "SecurePassword@123"
}
```

**Validation:**

- Username: 3-100 characters, alphanumeric + underscore
- Email: Valid email format, must be unique
- Password: Minimum 8 characters, must contain uppercase, lowercase, digit

**Response (201 Created):**

```json
{
  "success": true,
  "message": "User registered successfully",
  "data": {
    "id": 2,
    "username": "john_dev",
    "email": "john@example.com",
    "role": "Developer"
  }
}
```

**Error Responses:**

- `400 Bad Request`: Validation failed
- `409 Conflict`: Email or username already exists

### `POST /auth/login`

Login and receive JWT token.

**Request:**

```json
{
  "email": "john@example.com",
  "password": "SecurePassword@123"
}
```

**Response (200 OK):**

```json
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresIn": 86400,
    "user": {
      "id": 2,
      "username": "john_dev",
      "email": "john@example.com",
      "role": "Developer"
    }
  }
}
```

**Error Responses:**

- `401 Unauthorized`: Invalid credentials
- `404 Not Found`: User not found

---

## Issues Endpoints

### `GET /issues`

List all issues with optional filtering and pagination.

**Query Parameters:**

```
?page=1
&pageSize=10
&status=Open
&priority=High
&severity=Critical
&assignedUserId=1
&searchTerm=login
&sortBy=createdAt
&sortOrder=desc
```

**Headers (Required):**

```
Authorization: Bearer {token}
```

**Response (200 OK):**

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1,
        "title": "Login page not responding on mobile",
        "description": "Users report that the login page hangs...",
        "status": "Open",
        "priority": "High",
        "severity": "Critical",
        "createdByUserId": 1,
        "createdByUser": {
          "id": 1,
          "username": "admin",
          "email": "admin@bugtracker.com"
        },
        "assignedToUserId": 2,
        "assignedToUser": {
          "id": 2,
          "username": "john_dev",
          "email": "john@example.com"
        },
        "createdAt": "2024-04-01T10:30:00Z",
        "updatedAt": "2024-04-05T14:20:00Z",
        "commentCount": 3,
        "attachmentCount": 1
      }
    ],
    "totalCount": 15,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 2
  }
}
```

### `POST /issues`

Create a new issue.

**Headers (Required):**

```
Authorization: Bearer {token}
Content-Type: application/json
```

**Request:**

```json
{
  "title": "Dashboard loading slowly",
  "description": "Dashboard takes 5+ seconds to load. Need to optimize queries.",
  "priority": "High",
  "severity": "Major",
  "assignedToUserId": 2
}
```

**Validation:**

- Title: 5-300 characters (required)
- Description: Optional, max 5000 characters
- Priority: Open, Medium, High (default: Medium)
- Severity: Minor, Major, Critical (default: Minor)
- assignedToUserId: Optional, valid user ID

**Response (201 Created):**

```json
{
  "success": true,
  "data": {
    "id": 25,
    "title": "Dashboard loading slowly",
    "description": "Dashboard takes 5+ seconds to load...",
    "status": "Open",
    "priority": "High",
    "severity": "Major",
    "createdByUserId": 1,
    "createdByUser": { "id": 1, "username": "admin" },
    "assignedToUserId": 2,
    "createdAt": "2024-04-06T12:00:00Z",
    "updatedAt": "2024-04-06T12:00:00Z"
  }
}
```

### `GET /issues/{id}`

Get detailed information about a specific issue.

**Headers (Required):**

```
Authorization: Bearer {token}
```

**URL Parameters:**

```
id = 1 (integer, required)
```

**Response (200 OK):**

```json
{
  "success": true,
  "data": {
    "id": 1,
    "title": "Login page not responding on mobile",
    "description": "Full description...",
    "status": "Open",
    "priority": "High",
    "severity": "Critical",
    "createdByUserId": 1,
    "createdByUser": { "id": 1, "username": "admin" },
    "assignedToUserId": 2,
    "assignedToUser": { "id": 2, "username": "john_dev" },
    "createdAt": "2024-04-01T10:30:00Z",
    "updatedAt": "2024-04-05T14:20:00Z",
    "comments": [
      {
        "id": 1,
        "userId": 2,
        "user": { "username": "john_dev" },
        "text": "I've reproduced this issue...",
        "createdAt": "2024-04-02T09:00:00Z"
      }
    ],
    "attachments": [
      {
        "id": 1,
        "fileName": "screenshot.png",
        "filePath": "/uploads/abc123.png",
        "fileSize": 245632,
        "contentType": "image/png",
        "uploadedAt": "2024-04-02T10:00:00Z"
      }
    ],
    "activityLogs": [
      {
        "id": 1,
        "userId": 1,
        "action": "created_issue",
        "details": "Issue created",
        "createdAt": "2024-04-01T10:30:00Z"
      }
    ]
  }
}
```

**Error Responses:**

- `404 Not Found`: Issue doesn't exist
- `403 Forbidden`: Not authorized to view

### `PUT /issues/{id}`

Update an issue.

**Headers (Required):**

```
Authorization: Bearer {token}
Content-Type: application/json
```

**Request:**

```json
{
  "title": "Updated title",
  "description": "Updated description",
  "status": "In Progress",
  "priority": "High",
  "severity": "Critical",
  "assignedToUserId": 3
}
```

**Allowed Status Values:**

- `Open` - Issue is open and unresolved
- `In Progress` - Issue is being worked on
- `Resolved` - Issue is resolved

**Authorization:**

- Creator of the issue
- Assigned user
- Admin

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Issue updated successfully",
  "data": {
    "id": 1,
    "title": "Updated title",
    "status": "In Progress",
    ...
  }
}
```

### `DELETE /issues/{id}`

Delete an issue and all related data.

**Headers (Required):**

```
Authorization: Bearer {token}
```

**Authorization:**

- Creator of the issue (Owner)
- Admin only

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Issue deleted successfully"
}
```

**Error Responses:**

- `403 Forbidden`: Not authorized to delete
- `404 Not Found`: Issue not found

---

## Comments Endpoints

### `POST /comments`

Add a comment to an issue.

**Headers (Required):**

```
Authorization: Bearer {token}
Content-Type: application/json
```

**Request:**

```json
{
  "issueId": 1,
  "text": "This needs urgent attention. Users are reporting data loss."
}
```

**Validation:**

- issueId: Valid issue ID (required)
- text: 1-5000 characters (required)

**Response (201 Created):**

```json
{
  "success": true,
  "data": {
    "id": 15,
    "issueId": 1,
    "userId": 1,
    "user": { "id": 1, "username": "admin" },
    "text": "This needs urgent attention...",
    "createdAt": "2024-04-06T14:30:00Z"
  }
}
```

### `GET /comments/{issueId}`

Get all comments for an issue.

**Headers (Required):**

```
Authorization: Bearer {token}
```

**URL Parameters:**

```
issueId = 1 (integer, required)
```

**Response (200 OK):**

```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "issueId": 1,
      "userId": 2,
      "user": { "id": 2, "username": "john_dev" },
      "text": "I've reproduced the issue...",
      "createdAt": "2024-04-02T09:00:00Z"
    },
    {
      "id": 2,
      "issueId": 1,
      "userId": 1,
      "user": { "id": 1, "username": "admin" },
      "text": "Let's trace the network requests...",
      "createdAt": "2024-04-03T08:00:00Z"
    }
  ]
}
```

### `DELETE /comments/{id}`

Delete a comment.

**Headers (Required):**

```
Authorization: Bearer {token}
```

**Authorization:**

- Comment author
- Admin

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Comment deleted successfully"
}
```

---

## Attachments Endpoints

### `POST /attachments/{issueId}`

Upload a file attachment to an issue.

**Headers (Required):**

```
Authorization: Bearer {token}
Content-Type: multipart/form-data
```

**Form Data:**

```
file: <binary file>
```

**Restrictions:**

- Max file size: 10 MB
- Allowed extensions: .jpg, .jpeg, .png, .gif, .pdf, .txt, .log, .zip
- Max filename length: 255 characters

**Response (200 OK):**

```json
{
  "success": true,
  "data": {
    "id": 5,
    "issueId": 1,
    "userId": 1,
    "fileName": "error_log.txt",
    "filePath": "/uploads/550e8400-e29b-41d4-a716-446655440000.txt",
    "fileSize": 15234,
    "contentType": "text/plain",
    "uploadedAt": "2024-04-06T15:00:00Z"
  }
}
```

**Error Responses:**

- `400 Bad Request`: File too large or invalid type
- `404 Not Found`: Issue not found

### `DELETE /attachments/{id}`

Delete an attachment.

**Headers (Required):**

```
Authorization: Bearer {token}
```

**Authorization:**

- Uploader of the file
- Admin

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Attachment deleted successfully"
}
```

---

## Activity Logs Endpoints

### `GET /activitylogs/{issueId}`

Get activity history for an issue.

**Headers (Required):**

```
Authorization: Bearer {token}
```

**URL Parameters:**

```
issueId = 1 (integer, required)
```

**Response (200 OK):**

```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "issueId": 1,
      "userId": 1,
      "user": { "id": 1, "username": "admin" },
      "action": "created_issue",
      "details": "Issue created with priority HIGH and severity CRITICAL",
      "createdAt": "2024-04-01T10:30:00Z"
    },
    {
      "id": 2,
      "issueId": 1,
      "userId": 1,
      "user": { "id": 1, "username": "admin" },
      "action": "assigned_issue",
      "details": "Assigned to john_dev",
      "createdAt": "2024-04-01T10:35:00Z"
    },
    {
      "id": 3,
      "issueId": 1,
      "userId": 2,
      "user": { "id": 2, "username": "john_dev" },
      "action": "status_changed",
      "details": "Status changed from Open to In Progress",
      "createdAt": "2024-04-05T14:00:00Z"
    }
  ]
}
```

---

## Users Endpoints

### `GET /users`

List all users (Admin only).

**Headers (Required):**

```
Authorization: Bearer {token}
```

**Query Parameters:**

```
?page=1
&pageSize=20
&role=Developer
```

**Response (200 OK):**

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1,
        "username": "admin",
        "email": "admin@bugtracker.com",
        "role": "Admin",
        "createdAt": "2024-01-01T00:00:00Z"
      },
      {
        "id": 2,
        "username": "john_dev",
        "email": "john@example.com",
        "role": "Developer",
        "createdAt": "2024-02-15T10:00:00Z"
      }
    ],
    "totalCount": 5,
    "pageNumber": 1,
    "pageSize": 20
  }
}
```

**Authorization:**

- Admin only

### `GET /users/{id}`

Get user profile information.

**Headers (Required):**

```
Authorization: Bearer {token}
```

**Response (200 OK):**

```json
{
  "success": true,
  "data": {
    "id": 2,
    "username": "john_dev",
    "email": "john@example.com",
    "role": "Developer",
    "createdAt": "2024-02-15T10:00:00Z",
    "issuesCreated": 8,
    "issuesAssigned": 5,
    "commentsCount": 12
  }
}
```

---

## Error Responses

### Standard Error Format

```json
{
  "success": false,
  "message": "Error message describing what went wrong",
  "errors": [
    {
      "field": "fieldName",
      "message": "Specific validation error"
    }
  ]
}
```

### Common HTTP Status Codes

| Status | Meaning                                  |
| ------ | ---------------------------------------- |
| 200    | OK - Request successful                  |
| 201    | Created - Resource created               |
| 400    | Bad Request - Invalid input              |
| 401    | Unauthorized - Missing/invalid token     |
| 403    | Forbidden - Not authorized for action    |
| 404    | Not Found - Resource doesn't exist       |
| 409    | Conflict - Resource already exists       |
| 422    | Unprocessable Entity - Validation failed |
| 500    | Internal Server Error - Server error     |

---

## Rate Limiting

Currently, no rate limiting is implemented. It's recommended to add rate limiting in production:

```bash
# Suggested: 100 requests per minute per IP
# 1000 requests per hour per IP
```

---

## Testing

### Using cURL

```bash
# Register
curl -X POST https://localhost:7123/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username":"test_user",
    "email":"test@example.com",
    "password":"Password@123"
  }'

# Login
RESPONSE=$(curl -X POST https://localhost:7123/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email":"test@example.com",
    "password":"Password@123"
  }')
TOKEN=$(echo $RESPONSE | jq -r '.data.token')

# Create Issue
curl -X POST https://localhost:7123/api/issues \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title":"Test Issue",
    "description":"This is a test issue",
    "priority":"High",
    "severity":"Major"
  }'

# Get Issues
curl -X GET "https://localhost:7123/api/issues?page=1" \
  -H "Authorization: Bearer $TOKEN"
```

### Using Swagger UI

Navigate to `https://localhost:7123/swagger` for interactive API testing.

---

## Changelog

### Version 1.0.0 (April 2024)

- Initial release
- Full CRUD operations for issues, comments, attachments
- JWT authentication and authorization
- Activity logging
- File upload support
- Pagination and filtering
- Database seeding with demo data
