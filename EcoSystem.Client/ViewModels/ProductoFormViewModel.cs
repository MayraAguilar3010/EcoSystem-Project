using EcoSystem.Client.Models;
using EcoSystem.Client.Services;
using System.Globalization;

namespace EcoSystem.Client.ViewModels;

public class ProductoFormViewModel : BaseViewModel, IQueryAttributable
{
    private readonly ApiService _apiService;
    private int _id;
    private string _nombre = string.Empty;
    private string _precio = string.Empty;
    private string _stock = string.Empty;
    private string _title = "Nuevo producto";

    public int Id { get => _id; set { if (SetProperty(ref _id, value)) OnPropertyChanged(nameof(IsEditMode)); } }
    public string Nombre { get => _nombre; set => SetProperty(ref _nombre, value); }
    public string Precio { get => _precio; set => SetProperty(ref _precio, value); }
    public string Stock { get => _stock; set => SetProperty(ref _stock, value); }
    public string Title { get => _title; set => SetProperty(ref _title, value); }
    public bool IsEditMode => Id > 0;
    public AsyncCommand SaveCommand { get; }
    public AsyncCommand CancelCommand { get; }

    public ProductoFormViewModel(ApiService apiService)
    {
        _apiService = apiService;
        SaveCommand = new AsyncCommand(SaveAsync);
        CancelCommand = new AsyncCommand(CancelAsync);
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        Id = GetInt(query, "id");
        Nombre = GetString(query, "nombre");
        Precio = GetString(query, "precio");
        Stock = GetString(query, "stock");
        Title = IsEditMode ? "Editar producto" : "Nuevo producto";
    }

    private async Task SaveAsync()
    {
        if (!Validate(out var producto)) return;
        if (!BeginWork()) return;
        try
        {
            if (IsEditMode)
            {
                await _apiService.UpdateProductoAsync(Id, producto);
                SuccessMessage = "Producto actualizado.";
            }
            else
            {
                await _apiService.CreateProductoAsync(producto);
                SuccessMessage = "Producto creado.";
            }
            await Shell.Current.GoToAsync("..");
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

    private Task CancelAsync() => Shell.Current.GoToAsync("..");

    private bool Validate(out Producto producto)
    {
        producto = new Producto { Id = Id };
        if (string.IsNullOrWhiteSpace(Nombre))
        {
            ErrorMessage = "El nombre es obligatorio.";
            return false;
        }
        if (!decimal.TryParse(Precio, NumberStyles.Number, CultureInfo.InvariantCulture, out var precio) || precio <= 0)
        {
            ErrorMessage = "El precio debe ser mayor que cero. Usa punto decimal si es necesario.";
            return false;
        }
        if (!int.TryParse(Stock, NumberStyles.Integer, CultureInfo.InvariantCulture, out var stock) || stock < 0)
        {
            ErrorMessage = "Las existencias no pueden ser negativas.";
            return false;
        }
        producto.Nombre = Nombre.Trim();
        producto.Precio = precio;
        producto.Stock = stock;
        return true;
    }

    private static string GetString(IDictionary<string, object> query, string key) =>
        query.TryGetValue(key, out var value) ? Uri.UnescapeDataString(value?.ToString() ?? string.Empty) : string.Empty;

    private static int GetInt(IDictionary<string, object> query, string key) =>
        int.TryParse(GetString(query, key), out var value) ? value : 0;
}
