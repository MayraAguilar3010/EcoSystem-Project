using EcoSystem.Client.Models;

namespace EcoSystem.Client.Services;

public class TokenService
{
    private const string TokenKey = "ecosystem.jwt";
    private const string ExpiresKey = "ecosystem.jwt.expires";
    private const string UsernameKey = "ecosystem.username";
    private const string RoleKey = "ecosystem.role";

    public async Task SaveTokenAsync(LoginResponse login)
    {
        await SecureStorage.SetAsync(TokenKey, login.Token);
        await SecureStorage.SetAsync(ExpiresKey, login.ExpiresAt.ToUniversalTime().ToString("O"));
        await SecureStorage.SetAsync(UsernameKey, login.Username);
        await SecureStorage.SetAsync(RoleKey, login.Role);
    }

    public async Task<string?> GetTokenAsync()
    {
        if (!await IsAuthenticatedAsync())
        {
            return null;
        }

        return await SecureStorage.GetAsync(TokenKey);
    }

    public async Task<string?> GetRoleAsync() => await SecureStorage.GetAsync(RoleKey);
    public async Task<string?> GetUsernameAsync() => await SecureStorage.GetAsync(UsernameKey);

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await SecureStorage.GetAsync(TokenKey);
        var expiresText = await SecureStorage.GetAsync(ExpiresKey);
        if (string.IsNullOrWhiteSpace(token) || !DateTime.TryParse(expiresText, out var expiresAt))
        {
            await ClearAsync();
            return false;
        }

        if (expiresAt.ToUniversalTime() <= DateTime.UtcNow)
        {
            await ClearAsync();
            return false;
        }

        return true;
    }

    public Task ClearAsync()
    {
        SecureStorage.Remove(TokenKey);
        SecureStorage.Remove(ExpiresKey);
        SecureStorage.Remove(UsernameKey);
        SecureStorage.Remove(RoleKey);
        return Task.CompletedTask;
    }
}
