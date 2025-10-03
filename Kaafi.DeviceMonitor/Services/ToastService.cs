using System;
using System.Threading.Tasks;

namespace Kaafi.DeviceMonitor.Services
{
    public class ToastService
    {
        public event Action<string, string, string>? OnShow;

        // ✅ Synchronous helpers (for DeviceApiService and others)
        public void ShowSuccess(string message, string title = "Success")
        {
            OnShow?.Invoke("success", message, title);
        }

        public void ShowError(string message, string title = "Error")
        {
            OnShow?.Invoke("error", message, title);
        }

        public void ShowInfo(string message, string title = "Info")
        {
            OnShow?.Invoke("info", message, title);
        }

        public void ShowWarning(string message, string title = "Warning")
        {
            OnShow?.Invoke("warning", message, title);
        }

        // ✅ Async wrappers (for Employees.razor etc.)
        public Task ShowSuccessAsync(string message, string title = "Success")
        {
            ShowSuccess(message, title);
            return Task.CompletedTask;
        }

        public Task ShowErrorAsync(string message, string title = "Error")
        {
            ShowError(message, title);
            return Task.CompletedTask;
        }

        public Task ShowInfoAsync(string message, string title = "Info")
        {
            ShowInfo(message, title);
            return Task.CompletedTask;
        }

        public Task ShowWarningAsync(string message, string title = "Warning")
        {
            ShowWarning(message, title);
            return Task.CompletedTask;
        }
    }
}
