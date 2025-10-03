using System.Net.Http.Json;
using Kaafi.DeviceMonitor.Models;

namespace Kaafi.DeviceMonitor.Services;

public class EnrollmentApiService
{
    private readonly HttpClient _httpClient;

    public EnrollmentApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Enrollment>> GetEnrollmentsAsync()
    {
        var enrollments = await _httpClient.GetFromJsonAsync<List<Enrollment>>("api/enrollments");
        return enrollments ?? new List<Enrollment>();
    }

    public async Task<Enrollment> CreateEnrollmentAsync(Enrollment enrollment)
    {
        var response = await _httpClient.PostAsJsonAsync("api/enrollments", enrollment);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Enrollment>() ?? enrollment;
    }

    public async Task CreateEnrollmentsAsync(Enrollment[] enrollments)
    {
        var response = await _httpClient.PostAsJsonAsync("api/enrollments/bulk", enrollments);
        response.EnsureSuccessStatusCode();
    }
}