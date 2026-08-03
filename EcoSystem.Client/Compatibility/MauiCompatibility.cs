using System.Globalization;

namespace EcoSystem.Client.Compatibility;

public class ContentPage
{
    public object? BindingContext { get; set; }
    protected virtual void OnAppearing() { }
    protected void InitializeComponent() { }
}

public class Application
{
    protected void InitializeComponent() { }
    protected virtual Window CreateWindow(IActivationState? activationState) => new(null);
}

public sealed class Window
{
    public Window(object? page) => Page = page;
    public object? Page { get; }
}

public interface IActivationState { }

public class Shell
{
    public static Shell Current { get; } = new();
    public static void RegisterRoute(string route, Type pageType) { }
    public Task GoToAsync(string route) => Task.CompletedTask;
    public Task<bool> DisplayAlert(string title, string message, string accept, string cancel) => Task.FromResult(true);
}

public interface IQueryAttributable
{
    void ApplyQueryAttributes(IDictionary<string, object> query);
}

public interface IValueConverter
{
    object Convert(object? value, Type targetType, object? parameter, CultureInfo culture);
    object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture);
}

public static class SecureStorage
{
    private static readonly Dictionary<string, string> Values = new();

    public static Task SetAsync(string key, string value)
    {
        Values[key] = value;
        return Task.CompletedTask;
    }

    public static Task<string?> GetAsync(string key) => Task.FromResult(Values.TryGetValue(key, out var value) ? value : null);
    public static bool Remove(string key) => Values.Remove(key);
}
