namespace LMS.Blazor.Client.Services;

public class ToastService
{
    public event Action<string, ToastType>? OnShow;

    public void ShowToast(string message, ToastType type)
    {
        OnShow?.Invoke(message, type);
    }
}

public enum ToastType { Success, Danger }
