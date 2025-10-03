using System.Net.Http.Json;
using Kaafi.DeviceMonitor.Models;

namespace Kaafi.DeviceMonitor.Services;

public class EmployeeApiService
{
    private readonly HttpClient _httpClient;

    public EmployeeApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<(List<Employee> employees, int totalCount)> GetEmployeesAsync(string? search = null, int page = 1, int pageSize = 12)
    {
        var url = $"api/employees?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search))
        {
            url += $"&search={Uri.EscapeDataString(search)}";
        }

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var employees = await response.Content.ReadFromJsonAsync<List<Employee>>() ?? new List<Employee>();
        var totalCount = int.Parse(response.Headers.GetValues("X-Total-Count").FirstOrDefault() ?? "0");

        return (employees, totalCount);
    }

    public async Task<Employee?> GetEmployeeAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<Employee>($"api/employees/{id}");
    }

    public async Task<Employee> CreateEmployeeAsync(Employee employee)
    {
        var response = await _httpClient.PostAsJsonAsync("api/employees", employee);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Employee>() ?? employee;
    }

    public async Task UpdateEmployeeAsync(Employee employee)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/employees/{employee.Id}", employee);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteEmployeeAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/employees/{id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteEmployeesAsync(int[] ids)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, "api/employees/bulk")
        {
            Content = JsonContent.Create(ids)
        };
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }
}