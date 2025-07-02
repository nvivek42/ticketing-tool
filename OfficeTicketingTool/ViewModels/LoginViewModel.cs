using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;

namespace OfficeTicketingTool.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private string _username;
        private string _errorMessage;
        private bool _isLoading;
        private bool _hasError;

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
                // Clear error when user starts typing
                if (HasError)
                {
                    HasError = false;
                    ErrorMessage = string.Empty;
                }
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
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

        public bool HasError
        {
            get => _hasError;
            set
            {
                _hasError = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoginCommand { get; }

        public event Action<string, string> LoginRequested;


        public event Action RequestPasswordClear;

        public ICommand RequestPasswordClearCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand<PasswordBox>(ExecuteLogin, CanExecuteLogin);
            RequestPasswordClearCommand = new RelayCommand(ExecuteRequestPasswordClear);

        }

        private void ExecuteRequestPasswordClear()
        {
            RequestPasswordClear?.Invoke();
        }
        private bool CanExecuteLogin(PasswordBox passwordBox)
        {
            return !IsLoading && !string.IsNullOrWhiteSpace(Username) &&
                   passwordBox != null && !string.IsNullOrWhiteSpace(passwordBox.Password);
        }

        private async void ExecuteLogin(PasswordBox passwordBox)
        {
            if (passwordBox == null) return;

            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;

            try
            {
                // Simulate login process
                await System.Threading.Tasks.Task.Delay(1000);

                // Basic validation (replace with actual authentication)
                if (Username == "admin" && passwordBox.Password == "password")
                {
                    LoginRequested?.Invoke(Username, passwordBox.Password);
                }
                else
                {
                    HasError = true;
                    ErrorMessage = "Invalid username or password";
                }
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Simple RelayCommand implementation
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute((T)parameter);
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}