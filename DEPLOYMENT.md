# BugTracker API - Deployment Guide

This guide provides step-by-step instructions for deploying the BugTracker API to various environments.

## Table of Contents

1. [Local Development](#local-development)
2. [Docker Deployment](#docker-deployment)
3. [Linux Server Deployment](#linux-server-deployment)
4. [Windows IIS Deployment](#windows-iis-deployment)
5. [Troubleshooting](#troubleshooting)

---

## Local Development

### Prerequisites

- .NET 8.0 SDK or later
- MySQL Server 5.7 or later
- Git

### Setup Steps

1. **Clone Repository**

   ```bash
   git clone https://github.com/yourusername/bugtracker.git
   cd BugTracker.API
   ```

2. **Install Dependencies**

   ```bash
   dotnet restore
   ```

3. **Configure Database**

   Edit `appsettings.json`:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Port=3306;Database=bugtracker_db;User=root;Password=your_password;"
     }
   }
   ```

4. **Create Database**

   ```bash
   mysql -u root -p -e "CREATE DATABASE bugtracker_db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"
   ```

5. **Run Application**

   ```bash
   dotnet run
   ```

   The application will start at `https://localhost:7123`

6. **Access Application**
   - Frontend: https://localhost:7123
   - Swagger API Docs: https://localhost:7123/swagger

7. **Demo Credentials**
   ```
   Email: admin@bugtracker.com
   Password: Admin@123
   ```

---

## Docker Deployment

### Prerequisites

- Docker and Docker Compose installed
- Git

### Using Docker Compose (Recommended)

This is the easiest way to deploy locally or in development.

```bash
# Clone and navigate to project
git clone https://github.com/yourusername/bugtracker.git
cd BugTracker.API

# Start services
docker-compose up -d

# View logs
docker-compose logs -f api

# Stop services
docker-compose down
```

**Access Points:**

- API: http://localhost:7123
- MySQL: localhost:3306 (host: mysql, user: root, password: root_password_123)

### Manual Docker Build

```bash
# Build image
docker build -t bugtracker-api:latest .

# Run with MySQL
docker run -d \
  --name bugtracker-api \
  -p 7123:8080 \
  -e "ConnectionStrings__DefaultConnection=Server=mysql-host;Port=3306;Database=bugtracker_db;User=root;Password=your_password;" \
  -e "JwtSettings__SecretKey=your-secret-key-here" \
  bugtracker-api:latest
```

### Docker Registry Push

```bash
# Tag image
docker tag bugtracker-api:latest myregistry.azurecr.io/bugtracker-api:latest

# Login to registry
az acr login --name myregistry

# Push
docker push myregistry.azurecr.io/bugtracker-api:latest
```

---

## Linux Server Deployment

### Prerequisites

- Ubuntu Server 20.04 LTS or later
- .NET 8.0 Runtime or SDK installed
- MySQL Server 5.7+
- Nginx or Apache (reverse proxy)
- SSL Certificate (Let's Encrypt recommended)

### Installation Steps

1. **Install .NET Runtime**

   ```bash
   sudo apt-get update
   sudo apt-get install -y dotnet-runtime-8.0
   ```

2. **Install MySQL Server**

   ```bash
   sudo apt-get install -y mysql-server
   sudo mysql_secure_installation
   ```

3. **Create Application User**

   ```bash
   sudo useradd -r -s /bin/bash bugtracker
   sudo mkdir -p /opt/bugtracker
   sudo chown bugtracker:bugtracker /opt/bugtracker
   ```

4. **Clone Repository**

   ```bash
   cd /opt/bugtracker
   sudo -u bugtracker git clone https://github.com/yourusername/bugtracker.git
   cd bugtracker/BugTracker.API
   ```

5. **Publish Application**

   ```bash
   sudo -u bugtracker dotnet publish -c Release -o /opt/bugtracker/publish
   ```

6. **Create Configuration File**

   ```bash
   sudo cp appsettings.Production.json /opt/bugtracker/publish/
   ```

   Edit the configuration:

   ```bash
   sudo nano /opt/bugtracker/publish/appsettings.Production.json
   ```

7. **Create SystemD Service**

   ```bash
   sudo tee /etc/systemd/system/bugtracker.service > /dev/null <<EOF
   [Unit]
   Description=BugTracker API
   After=network.target mysql.service

   [Service]
   Type=notify
   User=bugtracker
   WorkingDirectory=/opt/bugtracker/publish
   ExecStart=/usr/bin/dotnet /opt/bugtracker/publish/BugTracker.API.dll
   Restart=always
   RestartSec=10
   Environment="ASPNETCORE_ENVIRONMENT=Production"
   Environment="ASPNETCORE_URLS=http://localhost:5000"

   [Install]
   WantedBy=multi-user.target
   EOF
   ```

8. **Enable and Start Service**

   ```bash
   sudo systemctl daemon-reload
   sudo systemctl enable bugtracker
   sudo systemctl start bugtracker
   sudo systemctl status bugtracker
   ```

9. **Configure Nginx Reverse Proxy**

   ```bash
   sudo tee /etc/nginx/sites-available/bugtracker > /dev/null <<EOF
   server {
       listen 80;
       server_name yourdomain.com;

       location / {
           proxy_pass http://localhost:5000;
           proxy_http_version 1.1;
           proxy_set_header Upgrade \$http_upgrade;
           proxy_set_header Connection keep-alive;
           proxy_set_header Host \$host;
           proxy_cache_bypass \$http_upgrade;
       }
   }
   EOF
   ```

10. **Enable Nginx Site**

    ```bash
    sudo ln -s /etc/nginx/sites-available/bugtracker /etc/nginx/sites-enabled/
    sudo nginx -t
    sudo systemctl restart nginx
    ```

11. **Setup Let's Encrypt SSL**
    ```bash
    sudo apt-get install -y certbot python3-certbot-nginx
    sudo certbot --nginx -d yourdomain.com
    sudo systemctl restart nginx
    ```

### Verify Deployment

```bash
# Check service status
sudo systemctl status bugtracker

# View logs
sudo journalctl -u bugtracker -f

# Test API
curl -X GET https://yourdomain.com/swagger
```

---

## Windows IIS Deployment

### Prerequisites

- Windows Server 2019 or later
- IIS installed with URL Rewrite module
- .NET 8.0 Hosting Bundle installed
- MySQL installed and running

### Installation Steps

1. **Download .NET 8.0 Hosting Bundle**
   - Visit: https://dotnet.microsoft.com/download/dotnet/8.0
   - Download and install the Windows Hosting Bundle

2. **Create Application Folder**

   ```powershell
   New-Item -Path "C:\inetpub\bugtracker" -ItemType Directory
   ```

3. **Publish Application**

   ```powershell
   dotnet publish -c Release -o "C:\inetpub\bugtracker\publish"
   ```

4. **Configure appsettings.Production.json**

   ```powershell
   Copy-Item appsettings.Production.json "C:\inetpub\bugtracker\publish\"
   # Edit the file with your production settings
   ```

5. **Create IIS Application Pool**
   - Open IIS Manager
   - Right-click Application Pools → Add Application Pool
   - Name: `BugTrackerPool`
   - .NET CLR version: `No Managed Code`
   - Managed pipeline mode: `Integrated`

6. **Create IIS Website**
   - Right-click Sites → Add Website
   - Site name: `BugTracker`
   - Application pool: `BugTrackerPool`
   - Physical path: `C:\inetpub\bugtracker\publish`
   - Binding: `https://yourdomain.com` (add SSL certificate)

7. **Set ASPNETCORE_ENVIRONMENT**
   - In IIS Manager, select the application
   - Open Configuration Editor
   - Go to system.webServer/aspNetCore
   - Set environment variable: `ASPNETCORE_ENVIRONMENT = Production`

8. **Configure URL Rewrite (web.config automatic)**
   - The application should have web.config auto-generated
   - Verify it exists in publish folder

9. **Set Permissions**

   ```powershell
   # Give IIS Application Pool account permissions to upload folder
   $pool = "IIS AppPool\BugTrackerPool"
   $path = "C:\inetpub\bugtracker\publish\wwwroot\uploads"
   $acl = Get-Acl $path
   $permission = New-Object System.Security.AccessControl.FileSystemAccessRule($pool, "Modify", "ContainerInherit,ObjectInherit", "None", "Allow")
   $acl.SetAccessRule($permission)
   Set-Acl $path $acl
   ```

10. **Start Website**
    - In IIS Manager, click Start on the website

### Verify IIS Deployment

- Navigate to https://yourdomain.com
- Check Swagger: https://yourdomain.com/swagger
- View Application Pool events in Event Viewer

---

## Troubleshooting

### Database Connection Issues

**Error**: `"Unable to connect to MySQL server"`

**Solutions**:

```bash
# Check MySQL is running
sudo systemctl status mysql

# Verify connection string
# Contains: Server, Port, Database, User, Password

# Test connection
mysql -h hostname -u username -p
```

### Application Won't Start

**Check Logs**:

```bash
# Linux (SystemD)
sudo journalctl -u bugtracker -n 50

# Windows (Event Viewer)
# Application logs in Event Viewer

# Console output
dotnet publish/BugTracker.API.dll
```

### JWT Token Issues

**Verify JWT Configuration**:

- Secret key is same on all instances
- Secret key is at least 32 characters
- Token expiry is reasonable

### SSL Certificate Issues

**For Let's Encrypt (Linux)**:

```bash
# Renew certificates
sudo certbot renew

# Manual renewal
sudo certbot certonly --nginx -d yourdomain.com
```

**For IIS (Windows)**:

- Use IIS Certificate Binding interface
- Or use Certbot with IIS plugin

### File Upload Issues

**Verify Upload Folder Permissions**:

```bash
# Linux
sudo chown -R www-data:www-data /opt/bugtracker/publish/wwwroot/uploads
sudo chmod 755 /opt/bugtracker/publish/wwwroot/uploads

# Windows
# Use File Explorer → Right-click → Properties → Security
```

---

## Performance Optimization

### Database Optimization

```bash
# Add indexes for common queries
mysql> ALTER TABLE Issues ADD INDEX idx_status (Status);
mysql> ALTER TABLE Issues ADD INDEX idx_priority (Priority);
mysql> ALTER TABLE Issues ADD INDEX idx_assigned_user (AssignedToUserId);
```

### Application Caching

Update `Program.cs` to add response caching:

```csharp
builder.Services.AddResponseCaching();

// In middleware:
app.UseResponseCaching();
```

### Load Balancing

For multiple instances, use:

- **Linux**: Nginx with upstream servers
- **Windows**: IIS Application Request Routing (ARR)

---

## Monitoring & Maintenance

### Setup Application Insights (Azure)

```bash
# Add NuGet package
dotnet add package Microsoft.ApplicationInsights.AspNetCore
```

### Regular Backups

```bash
# MySQL backup
mysqldump -u root -p bugtracker_db > backup-$(date +%Y%m%d).sql

# Full server backup using tar
tar -czvf backup-$(date +%Y%m%d).tar.gz /opt/bugtracker
```

### Security Updates

```bash
# Update .NET runtime
sudo apt-get update
sudo apt-get upgrade dotnet-runtime-8.0

# Update MySQL
sudo apt-get upgrade mysql-server
```

---

## Support

For deployment issues, please refer to:

- Official .NET Documentation: https://docs.microsoft.com/dotnet/
- MySQL Documentation: https://dev.mysql.com/doc/
- IIS Documentation: https://docs.microsoft.com/iis/
