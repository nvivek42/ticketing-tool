using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using OfficeTicketingTool.Commands;
using OfficeTicketingTool.Services;

using static MaterialDesignThemes.Wpf.Theme;

namespace OfficeTicketingTool.ViewModels
{
    public class RegisterViewModel : INotifyPropertyChanged
    {
        private readonly IAuthService _authService;
        private string _username = string.Empty;
        private string _firstName = string.Empty;
        private string _lastName = string.Empty;
        private string _email = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _hasError;
        private bool _isLoading;

        public RegisterViewModel(IAuthService authService)
        {
            _authService = authService;
            RegisterCommand = new RelayCommand<System.Windows.Controls.PasswordBox>(
                async passwordBox => await RegisterAsync(passwordBox),
                passwordBox => CanRegister(passwordBox)
            );
            NavigateToLoginCommand = new RelayCommand(ExecuteNavigateToLogin);
        }

        public RegisterViewModel()
        {
            _authService = new AuthService(null, null);
            RegisterCommand = new RelayCommand<System.Windows.Controls.PasswordBox>(
                async passwordBox => await RegisterAsync(passwordBox),
                passwordBox => CanRegister(passwordBox)
            );
            NavigateToLoginCommand = new RelayCommand(ExecuteNavigateToLogin);
        }

        public event Action ShowLoginView;

        public ICommand NavigateToLoginCommand { get; }

        protected virtual void OnShowLoginView()
        {
            ShowLoginView?.Invoke();
        }

        private void ExecuteNavigateToLogin()
        {
            OnShowLoginView();
        }
        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string FirstName
        {
            get => _firstName;
            set
            {
                _firstName = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string LastName
        {
            get => _lastName;
            set
            {
                _lastName = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
                HasError = !string.IsNullOrEmpty(value);
            }
        }

        public bool HasError
        {
            get => _hasError;
            set
            {
                _hasError = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public ICommand RegisterCommand { get; }

        private async Task RegisterAsync(object parameter)
{
    if (!(parameter is System.Windows.Controls.PasswordBox passwordBox)) return;

    try
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        var user = await _authService.RegisterAsync(
            Username,
            passwordBox.Password,
            FirstName,
            LastName,
            Email);

        // Clear sensitive data
        passwordBox.Clear();
        Username = string.Empty;
        FirstName = string.Empty;
        LastName = string.Empty;
        Email = string.Empty;

        // Navigate back to login view after successful registration
        ShowLoginView?.Invoke();
    }
    catch (Exception ex)
    {
        ErrorMessage = ex.Message;
    }
    finally
    {
        IsLoading = false;
    }
}

        private bool CanRegister(object parameter)
        {
            return !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(FirstName) &&
                   !string.IsNullOrWhiteSpace(LastName) &&
                   !string.IsNullOrWhiteSpace(Email) &&
                   parameter is System.Windows.Controls.PasswordBox passwordBox &&
                   !string.IsNullOrWhiteSpace(passwordBox.Password);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}