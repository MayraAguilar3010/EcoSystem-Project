using EcoSystem.Client.Models;
using System.Net.Http.Json;

namespace EcoSystem.Client.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly TokenService _tokenService;

    public AuthService(HttpClient httpClient, TokenService tokenService)
    {
        _httpClient = httpClient;
        _tokenService = tokenService;
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public async Task<LoginResponse> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            throw new ApiException(400, "Captura usuario y contrasena.");
        }

        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/Auth/login", new LoginRequest
            {
                Username = username.Trim(),
                Password = password
            }, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException((int)response.StatusCode, (int)response.StatusCode == 401
                    ? "Usuario o contrasena incorrectos."
                    : $"La API respondio con codigo {(int)response.StatusCode}.");
            }

            var result = await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: cancellationToken)
                ?? throw new ApiException(500, "La respuesta de autenticacion no es valida.");

            await _tokenService.SaveTokenAsync(result);
            return result;
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new ApiException(408, "El inicio de sesion tardo demasiado.");
        }
        catch (HttpRequestException ex)
        {
            throw new ApiException(503, $"No fue posible conectar con la API: {ex.Message}");
        }
    }

    public Task LogoutAsync() => _tokenService.ClearAsync();
    public Task<string?> GetTokenAsync() => _tokenService.GetTokenAsync();
    public Task<bool> IsAuthenticatedAsync() => _tokenService.IsAuthenticatedAsync();
    public Task<bool> RestoreSessionAsync() => _tokenService.IsAuthenticatedAsync();
}

