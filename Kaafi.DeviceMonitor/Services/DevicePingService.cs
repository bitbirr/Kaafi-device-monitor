using System.Net.NetworkInformation;
using Kaafi.DeviceMonitor.Data;
using Kaafi.DeviceMonitor.Models;
using Microsoft.EntityFrameworkCore;

namespace Kaafi.DeviceMonitor.Services;

public class DevicePingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DevicePingService> _logger;
    private readonly TimeSpan _pingInterval = TimeSpan.FromMinutes(1);

    public DevicePingService(
        IServiceProvider serviceProvider,
        ILogger<DevicePingService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Device Ping Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            await PingAllDevicesAsync();
            await Task.Delay(_pingInterval, stoppingToken);
        }

        _logger.LogInformation("Device Ping Service is stopping.");
    }

    public async Task PingAllDevicesAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var devices = await context.Devices.ToListAsync();

        foreach (var device in devices)
        {
            var status = await PingDeviceAsync(device.IP);
            device.Status = status;
            
            if (status == "Online")
            {
                device.LastActive = DateTime.UtcNow;
            }

            // Log to history
            var history = new DeviceHistory
            {
                DeviceId = device.Id,
                Status = status,
                Timestamp = DateTime.UtcNow
            };

            context.DeviceHistories.Add(history);
        }

        await context.SaveChangesAsync();
        _logger.LogInformation($"Pinged {devices.Count} devices at {DateTime.UtcNow}");
    }

    public static async Task<string> PingDeviceAsync(string ipAddress)
    {
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(ipAddress, 5000);
            return reply.Status == IPStatus.Success ? "Online" : "Offline";
        }
        catch (Exception)
        {
            return "Offline";
        }
    }
}
