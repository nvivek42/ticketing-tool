using CommunityToolkit.Mvvm.ComponentModel;

namespace OfficeTicketingTool.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private string title = string.Empty;

        [ObservableProperty]
        private string statusMessage = string.Empty;
    }
}