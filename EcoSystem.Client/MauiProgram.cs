using EcoSystem.Client.Services;
using EcoSystem.Client.ViewModels;
using EcoSystem.Client.Views;

namespace EcoSystem.Client;

public static class MauiProgram
{
    private static readonly Uri ApiBaseAddress = new("https://ecosystem-connect-api.onrender.com/");

    public static ClientComposition CreateMauiApp()
    {
        var tokenService = new TokenService();
        var authService = new AuthService(new HttpClient { BaseAddress = ApiBaseAddress }, tokenService);
        var authHandler = new AuthHeaderHandler(tokenService) { InnerHandler = new HttpClientHandler() };
        var apiService = new ApiService(new HttpClient(authHandler) { BaseAddress = ApiBaseAddress });

        return new ClientComposition(
            tokenService,
            authService,
            apiService,
            () => new LoginPage(new LoginViewModel(authService)),
            () => new ProductosPage(new ProductosViewModel(apiService, authService, tokenService)),
            () => new ProductoFormPage(new ProductoFormViewModel(apiService)));
    }
}

public sealed record ClientComposition(
    TokenService TokenService,
    AuthService AuthService,
    ApiService ApiService,
    Func<LoginPage> LoginPageFactory,
    Func<ProductosPage> ProductosPageFactory,
    Func<ProductoFormPage> ProductoFormPageFactory);
