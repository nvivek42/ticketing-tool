using System;
using System.Windows;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.DependencyInjection;
using OfficeTicketingTool.Services;
using OfficeTicketingTool.ViewModels;
using OfficeTicketingTool.Views;

namespace OfficeTicketingTool
{
    public partial class MainWindow : Window
    {
        private readonly IAuthService _authService;
        private MainViewModel _mainViewModel;

        public MainWindow()
        {
            InitializeComponent();
            _authService = App.ServiceProvider.GetRequiredService<IAuthService>();
            _mainViewModel = App.ServiceProvider.GetRequiredService<MainViewModel>();
            DataContext = _mainViewModel;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Show login dialog if not authenticated
            if (!_authService.IsAuthenticated)
            {
                var loginViewModel = App.ServiceProvider.GetRequiredService<LoginViewModel>();
                var loginView = new LoginView(); // Parameterless constructor
                loginView.DataContext = loginViewModel; // Set ViewModel after instantiation

                var dialogWindow = new Window
                {
                    Content = loginView,
                    SizeToContent = SizeToContent.WidthAndHeight,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    Title = "Login"
                };

                if (dialogWindow.ShowDialog() != true || !_authService.IsAuthenticated)
                {
                    // Close application if login was cancelled or failed
                    Application.Current.Shutdown();
                    return;
                }

                // Initialize main view model after successful login
                await _mainViewModel.InitializeAsync();
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            
            // Handle global keyboard shortcuts
            if (e.Key == Key.F5)
            {
                _mainViewModel.LoadDataCommand?.Execute(null);
            }
            else if (e.Key == Key.Escape)
            {
                // Clear selection or close dialogs
                _mainViewModel.SelectedTicket = null;
            }
        }
    }
}
