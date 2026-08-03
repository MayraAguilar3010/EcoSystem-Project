using EcoSystem.Client.ViewModels;

namespace EcoSystem.Client.Views;

public partial class ProductoFormPage : ContentPage
{
    public ProductoFormPage(ProductoFormViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
