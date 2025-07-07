using OfficeTicketingTool.Models;
using OfficeTicketingTool.Services;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace OfficeTicketingTool.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
      public LoginViewModel()
        {
            
        }

        [ObservableProperty]
        private string _username;
        
        [ObservableProperty]
        private string _errorMessage;
        
        [ObservableProperty]
        private bool _isLoading;
        
        [ObservableProperty]
        private bool _hasError;
        private readonly IAuthService _authService;

        partial void OnErrorMessageChanged(string value)
        {
            OnPropertyChanged(nameof(HasError));
        }

        public ICommand LoginCommand { get; }

        
        public event Action<User> LoginSucceeded;

        public event Action RequestPasswordClear;

        public ICommand RequestPasswordClearCommand { get; }

        public ICommand NavigateToRegisterCommand { get; }

        public event Action ShowRegisterView;
        public LoginViewModel(IAuthService authService)
        {
            _authService = authService;
            LoginCommand = new RelayCommand<PasswordBox>(ExecuteLogin, CanExecuteLogin);
            RequestPasswordClearCommand = new RelayCommand(ExecuteRequestPasswordClear);
            NavigateToRegisterCommand = new RelayCommand(ExecuteNavigateToRegister);  

        }

        private void ExecuteNavigateToRegister()
        {
            ShowRegisterView?.Invoke();
        }
        private void ExecuteRequestPasswordClear()
        {
            RequestPasswordClear?.Invoke();
        }

        private bool CanExecuteLogin(PasswordBox passwordBox)
        {
            bool canExecute = !IsLoading && 
                           !string.IsNullOrWhiteSpace(Username) &&
                           passwordBox != null && 
                           !string.IsNullOrWhiteSpace(passwordBox.Password);
            
            Debug.WriteLine($"[Login] CanExecuteLogin - Username: {!string.IsNullOrEmpty(Username)}, Password: {passwordBox?.SecurePassword.Length > 0}, CanExecute: {canExecute}");
            return canExecute;
        }

        private async void ExecuteLogin(PasswordBox passwordBox)
        {
            Debug.WriteLine($"[Login] Login attempt started for user: {Username}");
            
            if (passwordBox == null)
            {
                Debug.WriteLine("[Login] Error: PasswordBox is null");
                return;
            }

            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;

            try
            {
                Debug.WriteLine("[Login] Calling authentication service...");
                var user = await _authService.AuthenticateAsync(Username, passwordBox.Password);
                
                if (user != null)
                {
                    Debug.WriteLine($"[Login] Authentication successful for user: {user.Username}");
                    // Raise the correct event for successful login
                    LoginSucceeded?.Invoke(user);
                }
                else
                {
                    Debug.WriteLine($"[Login] Authentication failed - Invalid credentials for user: {Username}");
                    HasError = true;
                    ErrorMessage = "Invalid username or password";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Login] Error during authentication: {ex}");
                HasError = true;
                ErrorMessage = "An error occurred during login. Please try again.";
            }
            finally
            {
                IsLoading = false;
                Debug.WriteLine("[Login] Login process completed");
            }
        }
       
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T?> _execute;
        private readonly Func<T?, bool>? _canExecute;

        public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            if (_canExecute == null) return true;
            
            if (parameter is T typedParam)
                return _canExecute(typedParam);
                
            return _canExecute(default);
        }

        public void Execute(object? parameter)
        {
            if (parameter is T typedParam)
                _execute(typedParam);
            else
                _execute(default);
        }

        public event EventHandler? CanExecuteChanged
        {
            add { if (value != null) CommandManager.RequerySuggested += value; }
            remove { if (value != null) CommandManager.RequerySuggested -= value; }
        }
    }
}