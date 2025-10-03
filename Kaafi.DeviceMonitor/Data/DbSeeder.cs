using Microsoft.AspNetCore.Identity;
using Kaafi.DeviceMonitor.Models;

namespace Kaafi.DeviceMonitor.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, IServiceProvider serviceProvider)
    {
        // Seed Identity users
        await SeedUsersAsync(serviceProvider);

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

    private static async Task SeedUsersAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Create Admin role if it doesn't exist
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        // Create default admin user
        var adminEmail = "admin@kaafi.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}
