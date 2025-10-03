namespace Kaafi.DeviceMonitor.Services;

public class ToastService
{
    public event Action<string, string, string>? OnShow;

    public void ShowSuccess(string message, string title = "Success")
    {
        OnShow?.Invoke(message, title, "success");
    }

    public void ShowError(string message, string title = "Error")
    {
        OnShow?.Invoke(message, title, "error");
    }

    public void ShowWarning(string message, string title = "Warning")
    {
        OnShow?.Invoke(message, title, "warning");
    }

    public void ShowInfo(string message, string title = "Info")
    {
        OnShow?.Invoke(message, title, "info");
    }
}