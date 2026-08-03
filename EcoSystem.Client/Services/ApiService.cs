using EcoSystem.Client.Models;
using System.Net.Http.Json;

namespace EcoSystem.Client.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public Task<List<Producto>> GetProductosAsync(CancellationToken cancellationToken = default) =>
        SendAsync<List<Producto>>(HttpMethod.Get, "api/Productos", null, cancellationToken);

    public Task<Producto> CreateProductoAsync(Producto producto, CancellationToken cancellationToken = default) =>
        SendAsync<Producto>(HttpMethod.Post, "api/Productos", producto, cancellationToken);

    public Task<Producto> UpdateProductoAsync(int id, Producto producto, CancellationToken cancellationToken = default) =>
        SendAsync<Producto>(HttpMethod.Put, $"api/Productos/{id}", producto, cancellationToken);

    public async Task DeleteProductoAsync(int id, CancellationToken cancellationToken = default)
    {
        await SendAsync<object>(HttpMethod.Delete, $"api/Productos/{id}", null, cancellationToken);
    }

    private async Task<T> SendAsync<T>(HttpMethod method, string uri, object? body, CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(method, uri);
            if (body is not null)
            {
                request.Content = JsonContent.Create(body);
            }

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException((int)response.StatusCode, GetMessage((int)response.StatusCode));
            }

            if (typeof(T) == typeof(object))
            {
                return default!;
            }

            var result = await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
            return result ?? throw new ApiException(500, "La API no devolvio datos validos.");
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new ApiException(408, "La solicitud tardo demasiado. Intenta de nuevo.");
        }
        catch (HttpRequestException ex)
        {
            throw new ApiException(503, $"No fue posible conectar con la API: {ex.Message}");
        }
    }

    private static string GetMessage(int statusCode) => statusCode switch
    {
        400 => "La solicitud no es valida. Revisa los datos capturados.",
        401 => "Tu sesion no es valida o expiro. Inicia sesion de nuevo.",
        403 => "Tu usuario no tiene permiso para realizar esta accion.",
        404 => "El recurso solicitado no existe.",
        >= 500 => "El servidor no pudo procesar la solicitud.",
        _ => $"La API respondio con codigo {statusCode}."
    };
}

