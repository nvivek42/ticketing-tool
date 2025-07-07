using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using OfficeTicketingTool.ViewModels;

namespace OfficeTicketingTool.Views
{
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            Debug.WriteLine("[LoginView] Initializing LoginView");
            InitializeComponent();
            Loaded += OnLoaded;
            Debug.WriteLine("[LoginView] LoginView initialized");
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[LoginView] OnLoaded event triggered");
            
            if (DataContext is LoginViewModel viewModel)
            {
                Debug.WriteLine("[LoginView] Setting up view model event handlers");
                viewModel.RequestPasswordClear += OnRequestPasswordClear;
                viewModel.LoginSucceeded += OnLoginSucceeded;
            }
            else
            {
                Debug.WriteLine("[LoginView] Warning: DataContext is not a LoginViewModel");
            }

            // Set focus to username field
            UsernameTextBox.Focus();
            Debug.WriteLine("[LoginView] Focus set to UsernameTextBox");
        }

        private void OnLoginSucceeded(Models.User user)
        {
            Debug.WriteLine($"[LoginView] Login succeeded for user: {user.Username}");
            // Add any additional UI updates after successful login
        }

        private void OnRequestPasswordClear()
        {
            Debug.WriteLine("[LoginView] Clearing password field");
            PasswordBox.Clear();
            PasswordBox.Focus();
            Debug.WriteLine("[LoginView] Focus set to PasswordBox after clear");
        }

        public LoginView(LoginViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }

        // Handle Enter key in password box to trigger login
        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Debug.WriteLine("[LoginView] Enter key pressed in PasswordBox");
                if (DataContext is LoginViewModel viewModel)
                {
                    bool canExecute = viewModel.LoginCommand.CanExecute(PasswordBox);
                    Debug.WriteLine($"[LoginView] LoginCommand.CanExecute: {canExecute}");
                    
                    if (canExecute)
                    {
                        Debug.WriteLine("[LoginView] Executing LoginCommand");
                        viewModel.LoginCommand.Execute(PasswordBox);
                    }
                    else
                    {
                        Debug.WriteLine("[LoginView] LoginCommand cannot be executed (missing username or password)");
                    }
                }
                else
                {
                    Debug.WriteLine("[LoginView] Error: DataContext is not a LoginViewModel");
                }
                e.Handled = true;
            }
        }
    }
}

