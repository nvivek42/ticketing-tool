using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using OfficeTicketingTool.ViewModels;

namespace OfficeTicketingTool.Views
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();

            // Set focus to username field when loaded
            Loaded += (s, e) =>
            {
                if (DataContext is LoginViewModel viewModel)
                {
                    // Set the password changed handler when view model is set
                    viewModel.RequestPasswordClear += () =>
                    {
                        PasswordBox.Clear();
                        PasswordBox.Focus();
                    };
                }

                // Set focus to username field
                var userNameBox = this.FindName("UserNameBox") as TextBox;
                userNameBox?.Focus();
            };
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