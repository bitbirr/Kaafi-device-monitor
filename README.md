# Kaafi Device Monitor

A .NET 8 Blazor Server application for monitoring network devices with automatic and manual ping capabilities.

## Features

- **Device Management**: Monitor multiple network devices with their IP addresses and ports
- **Automatic Ping Service**: Background service that pings all devices every minute
- **Manual Ping**: Manually trigger ping for any device with a single click
- **Status Tracking**: Visual status indicators (Online/Offline/Unknown)
- **History Tracking**: View detailed ping history for each device
- **Real-time Updates**: Blazor Server provides real-time UI updates
- **Database Persistence**: Uses Entity Framework Core with SQL Server/SQLite

## Technologies Used

- **.NET 8**: Latest .NET framework
- **Blazor Server**: Interactive server-side UI
- **Entity Framework Core 8**: ORM for database operations
- **SQL Server**: Production database (SQL Server 2019+)
- **SQLite**: Development/testing database
- **Bootstrap 5**: Responsive UI framework

## Database Schema

### Devices Table
- `Id` (int, Primary Key, Auto-increment)
- `Name` (string, Required, Max 100 chars)
- `IP` (string, Required, Max 50 chars)
- `Port` (int, Default: 4370)
- `Status` (string, Max 20 chars)
- `LastActive` (DateTime, Nullable)

### DeviceHistory Table
- `Id` (int, Primary Key, Auto-increment)
- `DeviceId` (int, Foreign Key)
- `Status` (string, Required, Max 20 chars)
- `Timestamp` (DateTime, Required)

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server 2019+ (for production) or SQLite (for development)

### Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/bitbirr/Kaafi-device-monitor.git
   cd Kaafi-device-monitor
   ```

2. Navigate to the project directory:
   ```bash
   cd Kaafi.DeviceMonitor
   ```

3. Update the connection string (optional):
   - For **SQL Server**: Edit `appsettings.json`
   - For **SQLite**: Edit `appsettings.Development.json` (default for development)

4. Run the application:
   ```bash
   dotnet run
   ```

5. Open your browser and navigate to:
   ```
   http://localhost:5051/devices
   ```

### Configuration

#### SQL Server (Production)
Edit `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=KaafiDeviceMonitor;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

#### SQLite (Development)
Edit `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=kaafi_device_monitor.db"
  }
}
```

## Usage

### Viewing Devices
Navigate to `/devices` to view all monitored devices in a table format showing:
- Serial Number (S.N)
- Device Name
- IP Address
- Port Number
- Current Status (with color-coded badges)
- Last Active Timestamp
- Action Buttons (Manual Ping, History)

### Manual Ping
Click the **"Manual Ping"** button next to any device to immediately check its status. The status will update in real-time.

### View History
Click the **"History"** button to open a modal showing the last 50 ping records for that device, including:
- Status (Online/Offline)
- Timestamp of each ping

### Automatic Background Service
The application automatically pings all devices every 60 seconds in the background. You can monitor the service logs in the console output.

## Project Structure

```
Kaafi.DeviceMonitor/
├── Components/
│   ├── Layout/
│   │   ├── MainLayout.razor
│   │   └── NavMenu.razor
│   └── Pages/
│       ├── Devices.razor        # Main device monitoring page
│       ├── Home.razor
│       ├── Counter.razor
│       └── Weather.razor
├── Data/
│   ├── ApplicationDbContext.cs  # EF Core DbContext
│   └── DbSeeder.cs              # Database seeder
├── Models/
│   ├── Device.cs                # Device entity
│   └── DeviceHistory.cs         # History entity
├── Services/
│   └── DevicePingService.cs     # Background ping service
├── Migrations/                   # EF Core migrations
├── wwwroot/                      # Static files
├── appsettings.json             # Production configuration
├── appsettings.Development.json # Development configuration
└── Program.cs                    # Application entry point
```

## Database Migrations

The application automatically applies migrations on startup. To manually manage migrations:

### Create a new migration:
```bash
dotnet ef migrations add MigrationName
```

### Apply migrations:
```bash
dotnet ef database update
```

### Remove last migration:
```bash
dotnet ef migrations remove
```

## Sample Data

The application seeds 5 sample devices on first run:
- Device 1: 192.168.1.100:4370
- Device 2: 192.168.1.101:4370
- Device 3: 192.168.1.102:4370
- Device 4: 192.168.1.103:4370
- Device 5: 192.168.1.104:4370

## Development

### Building the project:
```bash
dotnet build
```

### Running in watch mode (auto-reload):
```bash
dotnet watch run
```

### Running tests (if added):
```bash
dotnet test
```

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License.

## Acknowledgments

- Built with .NET 8 and Blazor Server
- Uses Bootstrap 5 for styling
- Entity Framework Core for data access