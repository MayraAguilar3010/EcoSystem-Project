namespace EcoSystem.Client.ViewModels;

public abstract class BaseViewModel : ObservableViewModel
{
    private bool _isBusy;
    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;

    public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }
    public string ErrorMessage { get => _errorMessage; set => SetProperty(ref _errorMessage, value); }
    public string SuccessMessage { get => _successMessage; set => SetProperty(ref _successMessage, value); }

    protected bool BeginWork()
    {
        if (IsBusy) return false;
        IsBusy = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
        return true;
    }

    protected void EndWork() => IsBusy = false;
}
