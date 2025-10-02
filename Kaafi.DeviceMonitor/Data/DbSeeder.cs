using Kaafi.DeviceMonitor.Models;

namespace Kaafi.DeviceMonitor.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Check if we already have data
        if (context.Devices.Any())
        {
            return;
        }

        var devices = new List<Device>
        {
            new Device
            {
                Name = "Device 1",
                IP = "192.168.1.100",
                Port = 4370,
                Status = "Unknown",
                LastActive = null
            },
            new Device
            {
                Name = "Device 2",
                IP = "192.168.1.101",
                Port = 4370,
                Status = "Unknown",
                LastActive = null
            },
            new Device
            {
                Name = "Device 3",
                IP = "192.168.1.102",
                Port = 4370,
                Status = "Unknown",
                LastActive = null
            },
            new Device
            {
                Name = "Device 4",
                IP = "192.168.1.103",
                Port = 4370,
                Status = "Unknown",
                LastActive = null
            },
            new Device
            {
                Name = "Device 5",
                IP = "192.168.1.104",
                Port = 4370,
                Status = "Unknown",
                LastActive = null
            }
        };

        await context.Devices.AddRangeAsync(devices);
        await context.SaveChangesAsync();
    }
}
