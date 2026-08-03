using EcoSystem.Client.ViewModels;

namespace EcoSystem.Client.Views;

public partial class ProductosPage : ContentPage
{
    private readonly ProductosViewModel _viewModel;

    public ProductosPage(ProductosViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeCommand.ExecuteAsync(null);
    }
}
