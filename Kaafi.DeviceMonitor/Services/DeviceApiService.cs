using System.Net.Http.Json;
using Kaafi.DeviceMonitor.Models;

namespace Kaafi.DeviceMonitor.Services;

public class DeviceApiService
{
    private readonly HttpClient _httpClient;
    private readonly ToastService _toastService;

    public DeviceApiService(HttpClient httpClient, ToastService toastService)
    {
        _httpClient = httpClient;
        _toastService = toastService;
    }

    public async Task<List<Device>> GetDevicesAsync(string? search = null)
    {
        var url = "api/devices";
        if (!string.IsNullOrWhiteSpace(search))
        {
            url += $"?search={Uri.EscapeDataString(search)}";
        }

        var devices = await _httpClient.GetFromJsonAsync<List<Device>>(url);
        return devices ?? new List<Device>();
    }

    public async Task<Device?> GetDeviceAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<Device>($"api/devices/{id}");
    }

    public async Task<Device?> CreateDeviceAsync(Device device)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/devices", device);
            if (response.IsSuccessStatusCode)
            {
                _toastService.ShowSuccess("Device created successfully");
                return await response.Content.ReadFromJsonAsync<Device>();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                _toastService.ShowError($"Failed to create device: {error}");
                return null;
            }
        }
        catch (Exception ex)
        {
            _toastService.ShowError($"Error creating device: {ex.Message}");
            return null;
        }
    }

    public async Task<string> PingDeviceAsync(int id)
    {
        var response = await _httpClient.PostAsync($"api/devices/{id}/ping", null);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<List<DeviceHistory>> GetDeviceHistoryAsync(int deviceId, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 20)
    {
        var url = $"api/devices/{deviceId}/history?page={page}&pageSize={pageSize}";
        if (startDate.HasValue)
        {
            url += $"&startDate={startDate.Value:yyyy-MM-dd}";
        }
        if (endDate.HasValue)
        {
            url += $"&endDate={endDate.Value:yyyy-MM-dd}";
        }

        var history = await _httpClient.GetFromJsonAsync<List<DeviceHistory>>(url);
        return history ?? new List<DeviceHistory>();
    }
}