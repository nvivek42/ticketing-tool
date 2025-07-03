using System;
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
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel viewModel)
            {
                // Set the password changed handler when view model is set
                viewModel.RequestPasswordClear += OnRequestPasswordClear;
            }

            // Set focus to username field
            UsernameTextBox.Focus();
        }

        private void OnRequestPasswordClear()
        {
            PasswordBox.Clear();
            PasswordBox.Focus();
        }

        public LoginView(LoginViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }

        // Handle Enter key in password box to trigger login
        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && DataContext is LoginViewModel viewModel)
            {
                if (viewModel.LoginCommand.CanExecute(PasswordBox))
                {
                    viewModel.LoginCommand.Execute(PasswordBox);
                }
                e.Handled = true;
            }
        }
    }
}

