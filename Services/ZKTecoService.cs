using System.Runtime.InteropServices;

namespace Kaafi.DeviceMonitor.Services;

public interface IZKTecoService
{
    bool Connect(string ipAddress, int port);
    void Disconnect();
    bool IsConnected();
    string GetDeviceSerialNumber();
    bool BeginEnrollment(int userId, int fingerIndex);
    bool CaptureFingerprint(int captureNumber, out string template);
    bool SaveTemplate(int userId, int fingerIndex, string template);
    int GetEnrollDataCount();
}

public class ZKTecoService : IZKTecoService
{
    private dynamic? _zkDevice;
    private bool _isConnected;
    private string _currentDeviceId = string.Empty;
    private readonly ILogger<ZKTecoService> _logger;
    private readonly List<string> _capturedTemplates = new();
    private int _machineNumber = 1;

    public ZKTecoService(ILogger<ZKTecoService> logger)
    {
        _logger = logger;
        InitializeDevice();
    }

    private void InitializeDevice()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var zkemkeeperType = Type.GetTypeFromProgID("zkemkeeper.ZKEM");
                if (zkemkeeperType != null)
                {
                    _zkDevice = Activator.CreateInstance(zkemkeeperType);
                    _logger.LogInformation("ZKTeco SDK initialized successfully");
                }
                else
                {
                    _logger.LogWarning("zkemkeeper.ZKEM not registered. Running in mock mode.");
                }
            }
            else
            {
                _logger.LogWarning("Not running on Windows. ZKTeco SDK will run in mock mode.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to initialize ZKTeco SDK. Running in mock mode.");
        }
    }

    public bool Connect(string ipAddress, int port)
    {
        try
        {
            if (_zkDevice != null)
            {
                _isConnected = _zkDevice.Connect_Net(ipAddress, port);
                if (_isConnected)
                {
                    _currentDeviceId = GetDeviceSerialNumber();
                    _logger.LogInformation("Connected to device {DeviceId} at {IpAddress}:{Port}", 
                        _currentDeviceId, ipAddress, port);
                }
                else
                {
                    int errorCode = 0;
                    _zkDevice.GetLastError(ref errorCode);
                    _logger.LogError("Failed to connect to device. Error code: {ErrorCode}", errorCode);
                }
            }
            else
            {
                _logger.LogInformation("Mock mode: Simulating connection to {IpAddress}:{Port}", ipAddress, port);
                _isConnected = true;
                _currentDeviceId = $"MOCK-{ipAddress.Replace(".", "-")}";
            }

            return _isConnected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting to device");
            return false;
        }
    }

    public void Disconnect()
    {
        try
        {
            if (_zkDevice != null && _isConnected)
            {
                _zkDevice.Disconnect();
                _logger.LogInformation("Disconnected from device");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disconnecting from device");
        }
        finally
        {
            _isConnected = false;
            _currentDeviceId = string.Empty;
        }
    }

    public bool IsConnected()
    {
        return _isConnected;
    }

    public string GetDeviceSerialNumber()
    {
        if (_zkDevice != null && _isConnected)
        {
            try
            {
                string serialNumber = string.Empty;
                if (_zkDevice.GetSerialNumber(_machineNumber, out serialNumber))
                {
                    return serialNumber;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting device serial number");
            }
        }
        
        return _currentDeviceId;
    }

    public bool BeginEnrollment(int userId, int fingerIndex)
    {
        try
        {
            _capturedTemplates.Clear();
            
            if (_zkDevice != null && _isConnected)
            {
                if (_zkDevice.StartEnroll(userId, fingerIndex))
                {
                    _logger.LogInformation("Started enrollment for user {UserId} finger {FingerIndex}", 
                        userId, fingerIndex);
                    return true;
                }
                else
                {
                    int errorCode = 0;
                    _zkDevice.GetLastError(ref errorCode);
                    _logger.LogError("Failed to start enrollment. Error code: {ErrorCode}", errorCode);
                    return false;
                }
            }
            else
            {
                _logger.LogInformation("Mock mode: Started enrollment for user {UserId} finger {FingerIndex}", 
                    userId, fingerIndex);
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting enrollment");
            return false;
        }
    }

    public bool CaptureFingerprint(int captureNumber, out string template)
    {
        template = string.Empty;
        
        try
        {
            if (_zkDevice != null && _isConnected)
            {
                string tempData = string.Empty;
                int tempLength = 0;
                
                if (_zkDevice.StartIdentify())
                {
                    System.Threading.Thread.Sleep(2000);
                    
                    if (_zkDevice.GetUserTmpExStr(_machineNumber, captureNumber, 
                        out tempData, out tempLength))
                    {
                        _capturedTemplates.Add(tempData);
                        template = tempData;
                        _logger.LogInformation("Captured fingerprint {CaptureNumber}", captureNumber);
                        return true;
                    }
                }
            }
            else
            {
                template = $"MOCK_TEMPLATE_{captureNumber}_{Guid.NewGuid():N}";
                _capturedTemplates.Add(template);
                _logger.LogInformation("Mock mode: Captured fingerprint {CaptureNumber}", captureNumber);
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error capturing fingerprint");
            return false;
        }
    }

    public bool SaveTemplate(int userId, int fingerIndex, string template)
    {
        try
        {
            if (_zkDevice != null && _isConnected)
            {
                if (_zkDevice.SetUserTmpExStr(_machineNumber, userId, fingerIndex, 1, template))
                {
                    _logger.LogInformation("Saved template for user {UserId} finger {FingerIndex}", 
                        userId, fingerIndex);
                    return true;
                }
                else
                {
                    int errorCode = 0;
                    _zkDevice.GetLastError(ref errorCode);
                    _logger.LogError("Failed to save template. Error code: {ErrorCode}", errorCode);
                    return false;
                }
            }
            else
            {
                _logger.LogInformation("Mock mode: Saved template for user {UserId} finger {FingerIndex}", 
                    userId, fingerIndex);
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving template");
            return false;
        }
    }

    public int GetEnrollDataCount()
    {
        return _capturedTemplates.Count;
    }
}
