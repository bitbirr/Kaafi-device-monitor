# Kaafi Device Monitor - Deployment Guide

This guide covers various deployment scenarios for the Kaafi Device Monitor API.

## Table of Contents
1. [Local Development](#local-development)
2. [Docker Deployment](#docker-deployment)
3. [Windows Server Deployment](#windows-server-deployment)
4. [Linux Server Deployment](#linux-server-deployment)
5. [Production Configuration](#production-configuration)

---

## Local Development

### Prerequisites
- .NET 9.0 SDK
- SQL Server (optional)
- Visual Studio 2022 or VS Code (optional)

### Steps

1. **Clone the repository**
   ```bash
   git clone https://github.com/bitbirr/Kaafi-device-monitor.git
   cd Kaafi-device-monitor
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Update appsettings.Development.json** (if using SQL Server)
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=KaafiDeviceMonitor;Trusted_Connection=true;TrustServerCertificate=true;"
     }
   }
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

5. **Access the API**
   - Swagger UI: http://localhost:5000/swagger
   - API: http://localhost:5000/api

---

## Docker Deployment

### Using Docker Compose (Recommended)

The included `docker-compose.yml` sets up both the API and SQL Server.

1. **Build and run with Docker Compose**
   ```bash
   docker-compose up -d
   ```

2. **Access the API**
   - API: http://localhost:5000/api
   - SQL Server: localhost:1433

3. **Stop the services**
   ```bash
   docker-compose down
   ```

4. **View logs**
   ```bash
   docker-compose logs -f api
   ```

### Using Docker Only

1. **Build the Docker image**
   ```bash
   docker build -t kaafi-device-monitor .
   ```

2. **Run the container**
   ```bash
   docker run -d -p 5000:80 \
     -e ASPNETCORE_ENVIRONMENT=Production \
     --name kaafi-api \
     kaafi-device-monitor
   ```

3. **Stop the container**
   ```bash
   docker stop kaafi-api
   docker rm kaafi-api
   ```

---

## Windows Server Deployment

### Prerequisites
- Windows Server 2016 or later
- .NET 9.0 Runtime (ASP.NET Core)
- IIS 10 or later
- SQL Server 2016 or later (optional)
- ZKTeco ZKTime 5.0 SDK (for hardware integration)

### Steps

1. **Install .NET 9.0 Hosting Bundle**
   - Download from: https://dotnet.microsoft.com/download/dotnet/9.0
   - Run the installer
   - Restart IIS: `iisreset`

2. **Publish the application**
   ```powershell
   dotnet publish -c Release -o C:\inetpub\kaafi-device-monitor
   ```

3. **Configure IIS**
   - Open IIS Manager
   - Create a new Application Pool:
     - Name: KaafiDeviceMonitor
     - .NET CLR version: No Managed Code
   - Create a new Website:
     - Site name: Kaafi Device Monitor
     - Application pool: KaafiDeviceMonitor
     - Physical path: C:\inetpub\kaafi-device-monitor
     - Binding: http://*:80

4. **Configure appsettings.json**
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=KaafiDeviceMonitor;Trusted_Connection=true;TrustServerCertificate=true;"
     },
     "Jwt": {
       "SecretKey": "CHANGE_THIS_TO_A_SECURE_SECRET_KEY_IN_PRODUCTION_123456789",
       "Issuer": "KaafiDeviceMonitor",
       "Audience": "KaafiDeviceMonitorAPI"
     }
   }
   ```

5. **Set permissions**
   - Grant IIS_IUSRS read access to the application folder

6. **Install ZKTeco SDK** (for production with hardware)
   - Install ZKTime 5.0 SDK
   - Register zkemkeeper.dll: `regsvr32 zkemkeeper.dll`

7. **Start the website in IIS**

---

## Linux Server Deployment

### Prerequisites
- Ubuntu 20.04+ / Debian 11+ / CentOS 8+
- .NET 9.0 Runtime
- Nginx (reverse proxy)
- SQL Server 2019+ or PostgreSQL (optional)

### Steps

1. **Install .NET 9.0 Runtime**
   ```bash
   wget https://dot.net/v1/dotnet-install.sh
   chmod +x dotnet-install.sh
   ./dotnet-install.sh --channel 9.0 --runtime aspnetcore
   ```

2. **Create deployment directory**
   ```bash
   sudo mkdir -p /var/www/kaafi-device-monitor
   ```

3. **Publish the application**
   ```bash
   dotnet publish -c Release -o /var/www/kaafi-device-monitor
   ```

4. **Create systemd service file**
   ```bash
   sudo nano /etc/systemd/system/kaafi-device-monitor.service
   ```

   Content:
   ```ini
   [Unit]
   Description=Kaafi Device Monitor API
   After=network.target

   [Service]
   Type=notify
   WorkingDirectory=/var/www/kaafi-device-monitor
   ExecStart=/usr/bin/dotnet /var/www/kaafi-device-monitor/Kaafi.DeviceMonitor.dll
   Restart=always
   RestartSec=10
   KillSignal=SIGINT
   SyslogIdentifier=kaafi-device-monitor
   User=www-data
   Environment=ASPNETCORE_ENVIRONMENT=Production
   Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

   [Install]
   WantedBy=multi-user.target
   ```

5. **Enable and start the service**
   ```bash
   sudo systemctl enable kaafi-device-monitor
   sudo systemctl start kaafi-device-monitor
   sudo systemctl status kaafi-device-monitor
   ```

6. **Configure Nginx as reverse proxy**
   ```bash
   sudo nano /etc/nginx/sites-available/kaafi-device-monitor
   ```

   Content:
   ```nginx
   server {
       listen 80;
       server_name your-domain.com;
       
       location / {
           proxy_pass http://localhost:5000;
           proxy_http_version 1.1;
           proxy_set_header Upgrade $http_upgrade;
           proxy_set_header Connection keep-alive;
           proxy_set_header Host $host;
           proxy_cache_bypass $http_upgrade;
           proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
           proxy_set_header X-Forwarded-Proto $scheme;
       }
   }
   ```

7. **Enable the site**
   ```bash
   sudo ln -s /etc/nginx/sites-available/kaafi-device-monitor /etc/nginx/sites-enabled/
   sudo nginx -t
   sudo systemctl restart nginx
   ```

8. **Set up SSL with Let's Encrypt** (optional but recommended)
   ```bash
   sudo apt install certbot python3-certbot-nginx
   sudo certbot --nginx -d your-domain.com
   ```

---

## Production Configuration

### Security Considerations

1. **Change JWT Secret Key**
   ```json
   {
     "Jwt": {
       "SecretKey": "USE_A_STRONG_RANDOM_SECRET_KEY_AT_LEAST_32_CHARACTERS_LONG"
     }
   }
   ```

2. **Use HTTPS**
   - Configure SSL certificates
   - Enforce HTTPS redirection

3. **Configure CORS** (if needed)
   Edit `Program.cs`:
   ```csharp
   builder.Services.AddCors(options =>
   {
       options.AddPolicy("AllowSpecificOrigin", policy =>
       {
           policy.WithOrigins("https://your-frontend-domain.com")
                 .AllowAnyMethod()
                 .AllowAnyHeader();
       });
   });
   ```

4. **Use SQL Server in Production**
   Update `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=your-server;Database=KaafiDeviceMonitor;User Id=your-user;Password=your-password;TrustServerCertificate=true;Encrypt=true;"
     }
   }
   ```

5. **Enable Logging**
   ```json
   {
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft.AspNetCore": "Warning",
         "Microsoft.EntityFrameworkCore": "Warning"
       }
     }
   }
   ```

### Database Migration

For production, use Entity Framework migrations:

1. **Install EF Core tools**
   ```bash
   dotnet tool install --global dotnet-ef
   ```

2. **Create initial migration**
   ```bash
   dotnet ef migrations add InitialCreate
   ```

3. **Update database**
   ```bash
   dotnet ef database update
   ```

### Environment Variables

Instead of storing sensitive data in appsettings.json, use environment variables:

**Linux/Docker:**
```bash
export ConnectionStrings__DefaultConnection="Server=..."
export Jwt__SecretKey="your-secret-key"
```

**Windows:**
```powershell
$env:ConnectionStrings__DefaultConnection="Server=..."
$env:Jwt__SecretKey="your-secret-key"
```

### Monitoring

1. **Health Checks** (add to Program.cs):
   ```csharp
   builder.Services.AddHealthChecks()
       .AddDbContextCheck<ApplicationDbContext>();
   
   app.MapHealthChecks("/health");
   ```

2. **Application Insights** (optional):
   ```bash
   dotnet add package Microsoft.ApplicationInsights.AspNetCore
   ```

3. **Log aggregation**:
   - Use Serilog, NLog, or similar
   - Send logs to centralized service (e.g., Elasticsearch, Splunk)

### Backup Strategy

1. **Database backups**:
   - Daily automated backups
   - Off-site storage
   
2. **Configuration backups**:
   - Version control for appsettings files
   - Secure storage for secrets

### Performance Tuning

1. **Connection pooling**: Already configured in Entity Framework
2. **Response caching**: Add caching where appropriate
3. **Rate limiting**: Implement to prevent abuse
4. **Load balancing**: Use multiple instances behind a load balancer

---

## Troubleshooting

### Common Issues

1. **Port already in use**
   ```bash
   # Linux
   sudo lsof -i :5000
   sudo kill -9 <PID>
   
   # Windows
   netstat -ano | findstr :5000
   taskkill /PID <PID> /F
   ```

2. **Database connection issues**
   - Verify connection string
   - Check SQL Server service is running
   - Verify firewall rules

3. **ZKTeco SDK not working**
   - Windows only: Ensure zkemkeeper.dll is registered
   - Check device network connectivity
   - Verify device IP and port

4. **JWT authentication failing**
   - Verify secret key matches
   - Check token expiration
   - Ensure clock sync across servers

### Logs Location

- **Development**: Console output
- **Windows IIS**: C:\inetpub\logs
- **Linux systemd**: `journalctl -u kaafi-device-monitor -f`
- **Docker**: `docker logs <container-id>`

---

## Support

For issues and questions:
- GitHub Issues: https://github.com/bitbirr/Kaafi-device-monitor/issues
- Email: support@example.com

## License

MIT License - See LICENSE file for details
