using EcoSystem.Client.Services;
using EcoSystem.Client.Views;

namespace EcoSystem.Client.ViewModels;

public class LoginViewModel : BaseViewModel
{
    private readonly AuthService _authService;
    private string _username = string.Empty;
    private string _password = string.Empty;

    public string Username { get => _username; set => SetProperty(ref _username, value); }
    public string Password { get => _password; set => SetProperty(ref _password, value); }
    public AsyncCommand LoginCommand { get; }
    public AsyncCommand RestoreSessionCommand { get; }

    public LoginViewModel(AuthService authService)
    {
        _authService = authService;
        LoginCommand = new AsyncCommand(LoginAsync);
        RestoreSessionCommand = new AsyncCommand(RestoreSessionAsync);
    }

    private async Task RestoreSessionAsync()
    {
        if (await _authService.RestoreSessionAsync())
        {
            await Shell.Current.GoToAsync($"//{nameof(ProductosPage)}");
        }
    }

    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Captura usuario y contrasena.";
            return;
        }

        if (!BeginWork()) return;
        try
        {
            var login = await _authService.LoginAsync(Username, Password);
            Password = string.Empty;
            SuccessMessage = $"Bienvenida, {login.Username}.";
            await Shell.Current.GoToAsync($"//{nameof(ProductosPage)}");
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            EndWork();
        }
    }
}
