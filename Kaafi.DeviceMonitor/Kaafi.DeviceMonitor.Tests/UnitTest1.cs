using Kaafi.DeviceMonitor.Services;
using System.Net.Http;

namespace Kaafi.DeviceMonitor.Tests;

public class UnitTest1
{
    [Fact]
    public void EmployeeApiService_CanBeInstantiated()
    {
        // Arrange
        var httpClient = new HttpClient();

        // Act
        var service = new EmployeeApiService(httpClient);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void DeviceApiService_CanBeInstantiated()
    {
        // Arrange
        var httpClient = new HttpClient();

        // Act
        var service = new DeviceApiService(httpClient);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void EnrollmentApiService_CanBeInstantiated()
    {
        // Arrange
        var httpClient = new HttpClient();

        // Act
        var service = new EnrollmentApiService(httpClient);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void AttendanceApiService_CanBeInstantiated()
    {
        // Arrange
        var httpClient = new HttpClient();

        // Act
        var service = new AttendanceApiService(httpClient);

        // Assert
        Assert.NotNull(service);
    }
}
