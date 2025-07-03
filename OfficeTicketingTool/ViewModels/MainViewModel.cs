using Microsoft.Extensions.DependencyInjection;
using OfficeTicketingTool.Commands;
using OfficeTicketingTool.Models;
using OfficeTicketingTool.Models.Enums;
using OfficeTicketingTool.Services;
using OfficeTicketingTool.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace OfficeTicketingTool.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly ITicketService _ticketService;
        private readonly IUserService _userService;
        private readonly ICategoryService _categoryService;

        private object _currentView;
        private User _currentUser;
        private string _statusMessage = "Ready";
        private bool _isLoading;
        private ObservableCollection<Ticket> _tickets;
        private Ticket _selectedTicket;
        private ICommand _logoutCommand;

        public ObservableCollection<Ticket> Tickets
        {
            get => _tickets;
            private set => SetProperty(ref _tickets, value);
        }

        public Ticket SelectedTicket
        {
            get => _selectedTicket;
            set => SetProperty(ref _selectedTicket, value);
        }

        public object CurrentView
        {
            get => _currentView;
            private set => SetProperty(ref _currentView, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public User CurrentUser
        {
            get => _currentUser;
            private set
            {
                if (SetProperty(ref _currentUser, value))
                {
                    OnPropertyChanged(nameof(IsAdmin));
                    OnPropertyChanged(nameof(IsAgent));
                    OnPropertyChanged(nameof(IsRegularUser));
                }
            }
        }

        public bool IsAdmin => _currentUser?.Role == UserRole.Admin;
        public bool IsAgent => _currentUser?.Role == UserRole.Agent || IsAdmin;
        public bool IsRegularUser => _currentUser?.Role == UserRole.User;

        public ICommand ShowTicketsViewCommand { get; }
        public ICommand ShowUsersViewCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand LoadDataCommand { get; }
        public ICommand CreateTicketCommand { get; }
        public ICommand UpdateTicketStatusCommand { get; }
        public ICommand AssignTicketCommand { get; }

        public MainViewModel(ITicketService ticketService, IUserService userService, ICategoryService categoryService, IAuthService authService)
        {
            _ticketService = ticketService ?? throw new ArgumentNullException(nameof(ticketService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));

            // Initialize collections
            _tickets = new ObservableCollection<Ticket>();

            // Initialize commands
            LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);
            LogoutCommand = new RelayCommand(_ => Logout());
            CreateTicketCommand = new AsyncRelayCommand(CreateTicketAsync);
            UpdateTicketStatusCommand = new AsyncRelayCommand<TicketStatus>(UpdateTicketStatusAsync);
            AssignTicketCommand = new AsyncRelayCommand(AssignTicketAsync);
            ShowTicketsViewCommand = new RelayCommand(_ => ShowTicketsView());
            ShowUsersViewCommand = new RelayCommand(_ => ShowUsersView());

            // Set current user from auth service
            CurrentUser = _authService.CurrentUser;

            // Set default view
            ShowTicketsView();
        }

        public async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Initializing application...";

                // Validate current user
                if (_authService.CurrentUser == null)
                {
                    throw new InvalidOperationException("No authenticated user found");
                }

                CurrentUser = _authService.CurrentUser;
                StatusMessage = $"Welcome, {CurrentUser.Username}";

                // Load initial data
                StatusMessage = "Loading application data...";
                await LoadDataAsync();

                StatusMessage = "Ready";
            }
            catch (InvalidOperationException ex)
            {
                StatusMessage = $"Authentication Error: {ex.Message}";
                MessageBox.Show($"Authentication failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // Redirect to login
                Logout();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Initialization failed: {ex.Message}";
                MessageBox.Show($"Application failed to initialize:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                Console.WriteLine($"Full exception details: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Loading tickets...";

                if (CurrentUser == null)
                {
                    throw new InvalidOperationException("No current user available");
                }

                // Load tickets based on user role
                List<Ticket> tickets;
                if (CurrentUser.Role == UserRole.Admin)
                {
                    tickets = await _ticketService.GetAllTicketsAsync();
                }
                else if (CurrentUser.Role == UserRole.Agent)
                {
                    tickets = await _ticketService.GetAssignedTicketsAsync(CurrentUser.Id);
                }
                else
                {
                    tickets = await _ticketService.GetTicketsByUserAsync(CurrentUser.Id);
                }

                Tickets = new ObservableCollection<Ticket>(tickets);

                if (Tickets.Any())
                {
                    SelectedTicket = Tickets.First();
                }

                StatusMessage = $"Loaded {Tickets.Count} tickets";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading tickets: {ex.Message}";
                Console.WriteLine($"Error loading tickets: {ex}");

                // Show user-friendly message
                MessageBox.Show($"Failed to load tickets: {ex.Message}", "Data Load Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void Logout()
        {
            try
            {
                // Logout using AuthService
                _authService.Logout();
                
                // Get the application instance
                var app = Application.Current as App;
                if (app != null)
                {
                    // Use the centralized logout in App.xaml.cs
                    app.Logout();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during logout: {ex.Message}", "Logout Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task CreateTicketAsync()
        {
            try
            {
                StatusMessage = "Opening create ticket dialog...";

                // TODO: Implement create ticket dialog
                // For now, just show a placeholder message
                MessageBox.Show("Create ticket functionality will be implemented here.", "Create Ticket", MessageBoxButton.OK, MessageBoxImage.Information);

                StatusMessage = "Ready";
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error creating ticket: {ex.Message}";
                Console.WriteLine($"Error creating ticket: {ex}");
                MessageBox.Show($"Failed to create ticket: {ex.Message}", "Create Ticket Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task UpdateTicketStatusAsync(TicketStatus newStatus)
        {
            if (SelectedTicket == null)
            {
                MessageBox.Show("Please select a ticket first.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = "Updating ticket status...";

                SelectedTicket.Status = newStatus;
                await _ticketService.UpdateTicketAsync(SelectedTicket, CurrentUser.Id);

                // Refresh the ticket list
                await LoadDataAsync();

                StatusMessage = "Ticket status updated successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error updating ticket status: {ex.Message}";
                Console.WriteLine($"Error updating ticket status: {ex}");
                MessageBox.Show($"Failed to update ticket status: {ex.Message}", "Update Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task AssignTicketAsync()
        {
            if (SelectedTicket == null)
            {
                MessageBox.Show("Please select a ticket first.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = "Opening assign ticket dialog...";

                // TODO: Implement ticket assignment dialog
                // For now, just show a placeholder message
                MessageBox.Show("Ticket assignment functionality will be implemented here.", "Assign Ticket", MessageBoxButton.OK, MessageBoxImage.Information);

                StatusMessage = "Ready";
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error assigning ticket: {ex.Message}";
                Console.WriteLine($"Error assigning ticket: {ex}");
                MessageBox.Show($"Failed to assign ticket: {ex.Message}", "Assignment Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ShowTicketsView()
        {
            try
            {
                if (App.ServiceProvider != null)
                {
                    var ticketViewModel = App.ServiceProvider.GetRequiredService<TicketViewModel>();
                    CurrentView = new TicketView { DataContext = ticketViewModel };
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading tickets view: {ex.Message}";
                Console.WriteLine($"Error loading tickets view: {ex}");
            }
        }

        private void ShowUsersView()
        {
            try
            {
                if (App.ServiceProvider != null)
                {
                    var userViewModel = App.ServiceProvider.GetRequiredService<UserViewModel>();
                    CurrentView = new UserView { DataContext = userViewModel };
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading users view: {ex.Message}";
                Console.WriteLine($"Error loading users view: {ex}");
            }
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object>? _canExecute;

        public RelayCommand(Action execute) : this(_ => execute()) { }
        public RelayCommand(Action<object> execute, Predicate<object>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || (parameter != null && _canExecute(parameter));
        }

        public void Execute(object? parameter)
        {
            _execute(parameter!);
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}