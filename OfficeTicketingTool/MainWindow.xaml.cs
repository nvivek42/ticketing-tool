using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.DependencyInjection;
using OfficeTicketingTool.Services;
using OfficeTicketingTool.ViewModels;
using OfficeTicketingTool.Views;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OfficeTicketingTool
{
    public partial class MainWindow : Window
    {
        private readonly IAuthService _authService;
        private readonly MainViewModel _mainViewModel;

        public MainWindow()
        {
            InitializeComponent();
            _authService = App.ServiceProvider.GetRequiredService<IAuthService>();
            _mainViewModel = App.ServiceProvider.GetRequiredService<MainViewModel>();
            DataContext = _mainViewModel;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Window is managed by WPF's WindowState
            // MainViewModel initializes itself in its constructor
            // No need for additional initialization
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // Clean up resources if needed
            if (Application.Current != null)
            {
                Application.Current.Shutdown();
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