# Quick Start Guide

Get the Kaafi Device Monitor API up and running in 5 minutes.

## Option 1: Using .NET CLI (Development)

**Prerequisites:** .NET 9.0 SDK

```bash
# 1. Clone and navigate
git clone https://github.com/bitbirr/Kaafi-device-monitor.git
cd Kaafi-device-monitor

# 2. Run the application
dotnet run

# 3. Open your browser
# Swagger UI: http://localhost:5000/swagger
# API: http://localhost:5000/api
```

## Option 2: Using Docker Compose (Production-like)

**Prerequisites:** Docker and Docker Compose

```bash
# 1. Clone the repository
git clone https://github.com/bitbirr/Kaafi-device-monitor.git
cd Kaafi-device-monitor

# 2. Start with Docker Compose (includes SQL Server)
docker-compose up -d

# 3. Access the API
# API: http://localhost:5000/api
# SQL Server: localhost:1433
```

## Test the API

### 1. Get Authentication Token

```bash
curl -X POST http://localhost:5000/api/auth/token \
  -H "Content-Type: application/json" \
  -d '{"username":"biotime","password":"biotime9.5"}'
```

Save the `token` value from the response.

### 2. Connect to Device

```bash
TOKEN="your-token-here"

curl -X POST http://localhost:5000/api/connect \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"ipAddress":"192.168.1.201","port":4370}'
```

### 3. Run Complete Demo

```bash
# Make demo script executable
chmod +x demo.sh

# Run the demo
./demo.sh
```

## Default Credentials

- **Username:** `biotime`
- **Password:** `biotime9.5`

⚠️ **Change these in production!** Edit `Controllers/AuthController.cs`

## Mock Mode

The application runs in **mock mode** by default (simulates device operations):
- Works on all platforms (Windows, Linux, macOS)
- No hardware required for testing
- Generates fake fingerprint templates

For production with actual ZKTeco devices:
- Install ZKTeco ZKTime 5.0 SDK on Windows
- Register `zkemkeeper.dll`
- Connect to real device IP addresses

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/token` | Get JWT token |
| POST | `/api/connect` | Connect to device |
| GET | `/api/status` | Check device status |
| POST | `/api/start-enroll` | Start enrollment |
| POST | `/api/capture-fingerprint` | Capture fingerprint |
| POST | `/api/save-enroll` | Save enrollment |
| POST | `/api/disconnect` | Disconnect device |

## Database

**Default:** In-memory database (data lost on restart)

**Production:** Configure SQL Server in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=KaafiDeviceMonitor;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

## Next Steps

1. **API Testing:** See [API_TESTING.md](API_TESTING.md)
2. **Deployment:** See [DEPLOYMENT.md](DEPLOYMENT.md)
3. **Full Documentation:** See [README.md](README.md)

## Troubleshooting

**Port already in use?**
```bash
# Change port in Program.cs or use:
dotnet run --urls "http://localhost:5001"
```

**Can't connect to device?**
- Check device IP address
- Ensure device is on same network
- Verify firewall settings
- In mock mode, any IP works (creates simulated connection)

## Need Help?

- 📖 Full docs: [README.md](README.md)
- 🐛 Issues: https://github.com/bitbirr/Kaafi-device-monitor/issues
- 📧 Email: support@example.com
