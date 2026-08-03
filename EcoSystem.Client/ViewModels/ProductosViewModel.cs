using EcoSystem.Client.Models;
using EcoSystem.Client.Services;
using EcoSystem.Client.Views;
using System.Collections.ObjectModel;
using System.Globalization;

namespace EcoSystem.Client.ViewModels;

public class ProductosViewModel : BaseViewModel
{
    private readonly ApiService _apiService;
    private readonly AuthService _authService;
    private readonly TokenService _tokenService;
    private string _currentUser = string.Empty;
    private string _currentRole = string.Empty;

    public ObservableCollection<Producto> Productos { get; } = new();
    public string CurrentUser { get => _currentUser; set => SetProperty(ref _currentUser, value); }
    public string CurrentRole { get => _currentRole; set => SetProperty(ref _currentRole, value); }
    public AsyncCommand InitializeCommand { get; }
    public AsyncCommand LoadProductosCommand { get; }
    public AsyncCommand AddProductoCommand { get; }
    public AsyncCommand<Producto> EditProductoCommand { get; }
    public AsyncCommand<Producto> DeleteProductoCommand { get; }
    public AsyncCommand LogoutCommand { get; }

    public ProductosViewModel(ApiService apiService, AuthService authService, TokenService tokenService)
    {
        _apiService = apiService;
        _authService = authService;
        _tokenService = tokenService;
        InitializeCommand = new AsyncCommand(InitializeAsync);
        LoadProductosCommand = new AsyncCommand(LoadProductosAsync);
        AddProductoCommand = new AsyncCommand(AddProductoAsync);
        EditProductoCommand = new AsyncCommand<Producto>(EditProductoAsync);
        DeleteProductoCommand = new AsyncCommand<Producto>(DeleteProductoAsync);
        LogoutCommand = new AsyncCommand(LogoutAsync);
    }

    private async Task InitializeAsync()
    {
        CurrentUser = await _tokenService.GetUsernameAsync() ?? "usuario";
        CurrentRole = await _tokenService.GetRoleAsync() ?? string.Empty;
        await LoadProductosAsync();
    }

    private async Task LoadProductosAsync()
    {
        if (!BeginWork()) return;
        try
        {
            var productos = await _apiService.GetProductosAsync();
            Productos.Clear();
            foreach (var producto in productos) Productos.Add(producto);
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
            if (ex.StatusCode == 401) await LogoutAsync();
        }
        finally
        {
            EndWork();
        }
    }

    private Task AddProductoAsync() => Shell.Current.GoToAsync(nameof(ProductoFormPage));

    private Task EditProductoAsync(Producto? producto)
    {
        if (producto is null) return Task.CompletedTask;
        return Shell.Current.GoToAsync(CreateProductoFormRoute(producto));
    }

    private static string CreateProductoFormRoute(Producto producto) =>
        $"{nameof(ProductoFormPage)}?id={producto.Id}&nombre={Uri.EscapeDataString(producto.Nombre)}&precio={Uri.EscapeDataString(producto.Precio.ToString(CultureInfo.InvariantCulture))}&stock={producto.Stock}";

    private async Task DeleteProductoAsync(Producto? producto)
    {
        if (producto is null || IsBusy) return;
        var confirm = await Shell.Current.DisplayAlert("Eliminar producto", $"Deseas eliminar {producto.Nombre}?", "Eliminar", "Cancelar");
        if (!confirm) return;
        if (!BeginWork()) return;
        try
        {
            await _apiService.DeleteProductoAsync(producto.Id);
            Productos.Remove(producto);
            SuccessMessage = "Producto eliminado.";
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

    private async Task LogoutAsync()
    {
        await _authService.LogoutAsync();
        Productos.Clear();
        await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
    }
}
