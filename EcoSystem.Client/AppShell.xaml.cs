using EcoSystem.Client.Views;

namespace EcoSystem.Client;

public partial class AppShell : Shell
{
    public AppShell()
    {
        RegisterRoute(nameof(ProductoFormPage), typeof(ProductoFormPage));
    }
}
