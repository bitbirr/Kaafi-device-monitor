// Adding missing usings
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using Kaafi.DeviceMonitor.Models;

namespace Kaafi.DeviceMonitor.Services;

public class AttendanceApiService
{
    private readonly HttpClient _httpClient;

    public AttendanceApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<(List<Attendance> attendance, int totalCount)> GetAttendanceAsync(
        string? employeeFilter = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        string? deviceFilter = null,
        int page = 1,
        int pageSize = 20)
    {
        var url = $"api/attendance?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(employeeFilter))
        {
            url += $"&employeeFilter={Uri.EscapeDataString(employeeFilter)}";
        }
        if (dateFrom.HasValue)
        {
            url += $"&dateFrom={dateFrom.Value:yyyy-MM-dd}";
        }
        if (dateTo.HasValue)
        {
            url += $"&dateTo={dateTo.Value:yyyy-MM-dd}";
        }
        if (!string.IsNullOrWhiteSpace(deviceFilter))
        {
            url += $"&deviceFilter={deviceFilter}";
        }

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var attendance = await response.Content.ReadFromJsonAsync<List<Attendance>>() ?? new List<Attendance>();
        var totalCount = int.Parse(response.Headers.GetValues("X-Total-Count").FirstOrDefault() ?? "0");

        return (attendance, totalCount);
    }

    public async Task<Attendance> CreateAttendanceAsync(Attendance attendance)
    {
        var response = await _httpClient.PostAsJsonAsync("api/attendance", attendance);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Attendance>() ?? attendance;
    }

    public async Task<List<Device>> GetDevicesAsync()
    {
        var devices = await _httpClient.GetFromJsonAsync<List<Device>>("api/attendance/devices");
        return devices ?? new List<Device>();
    }
}