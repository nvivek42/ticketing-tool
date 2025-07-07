using Microsoft.Extensions.DependencyInjection;
using OfficeTicketingTool.Commands;
using OfficeTicketingTool.Models;
using OfficeTicketingTool.Models.Enums;
using OfficeTicketingTool.Services;
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
        private bool _isLoading;
        private bool _isLoggedIn;
        private ObservableCollection<Ticket> _tickets;
        private Ticket _selectedTicket;
        private List<Ticket> _cachedTickets;
        private DateTime _lastRefreshTime = DateTime.MinValue;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

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

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set => SetProperty(ref _isLoggedIn, value);
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

        public bool IsAdmin => CurrentUser?.Role == UserRole.Admin;
        public bool IsAgent => CurrentUser?.Role == UserRole.Agent || IsAdmin;
        public bool IsRegularUser => CurrentUser?.Role == UserRole.User;

        private bool _isTicketsViewActive = true;
        private bool _isUsersViewActive;

        public bool IsTicketsViewActive
        {
            get => _isTicketsViewActive;
            private set
            {
                if (SetProperty(ref _isTicketsViewActive, value) && value)
                {
                    _isUsersViewActive = false;
                    OnPropertyChanged(nameof(IsUsersViewActive));
                }
            }
        }

        public bool IsUsersViewActive
        {
            get => _isUsersViewActive;
            private set
            {
                if (SetProperty(ref _isUsersViewActive, value) && value)
                {
                    _isTicketsViewActive = false;
                    OnPropertyChanged(nameof(IsTicketsViewActive));
                }
            }
        }

        public ICommand ShowTicketsViewCommand { get; }
        public ICommand ShowUsersViewCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand LoadDataCommand { get; }
        public ICommand CreateTicketCommand { get; }
        public ICommand UpdateTicketStatusCommand { get; }
        public ICommand AssignTicketCommand { get; }
        public ICommand RefreshDataCommand { get; }

        public MainViewModel(ITicketService ticketService, IUserService userService,
                           ICategoryService categoryService, IAuthService authService)
        {
            _ticketService = ticketService ?? throw new ArgumentNullException(nameof(ticketService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));

            // Initialize collections
            _tickets = new ObservableCollection<Ticket>();

            // Initialize commands
            LoadDataCommand = new AsyncRelayCommand(() => LoadDataAsync());
            RefreshDataCommand = new AsyncRelayCommand(() => LoadDataAsync(forceRefresh: true));
            LogoutCommand = new AsyncRelayCommand(LogoutAsync); // Updated to use AsyncRelayCommand
            CreateTicketCommand = new AsyncRelayCommand(CreateTicketAsync);
            UpdateTicketStatusCommand = new AsyncRelayCommand<TicketStatus>(UpdateTicketStatusAsync);
            AssignTicketCommand = new AsyncRelayCommand(AssignTicketAsync);
            ShowTicketsViewCommand = new RelayCommand(ShowTicketsView);
            ShowUsersViewCommand = new RelayCommand(ShowUsersView);

            // Set current user from auth service
            CurrentUser = _authService.CurrentUser;
            IsLoggedIn = CurrentUser != null;

            // Set default view
            ShowTicketsView();
        }

        public void SetCurrentUser(User user)
        {
            CurrentUser = user;
            IsLoggedIn = user != null;
            _cachedTickets = null; 
        }

        public async Task InitializeAsync()
        {
            try
            {
                StatusMessage = "Initializing application...";

                

                StatusMessage = "Application initialized successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error initializing application: {ex.Message}";
                Console.WriteLine($"Error initializing MainViewModel: {ex.Message}");
                throw; 
            }
        }

        private async Task LoadDataAsync(bool forceRefresh = false)
        {
            try
            {
                if (CurrentUser == null)
                {
                    throw new UnauthorizedAccessException("User not authenticated");
                }

                // Check cache first
                if (!forceRefresh &&
                    _cachedTickets != null &&
                    DateTime.Now - _lastRefreshTime < CacheDuration)
                {
                    Tickets = new ObservableCollection<Ticket>(_cachedTickets);
                    StatusMessage = $"Showing {Tickets.Count} tickets (cached)";
                    return;
                }

                IsLoading = true;
                StatusMessage = "Loading tickets...";

                // Get tickets based on user role
                var tickets = await _ticketService.GetTicketsForCurrentUserAsync(CurrentUser);

                // Update cache
                _cachedTickets = tickets.ToList();
                _lastRefreshTime = DateTime.Now;

                // Update UI
                Tickets = new ObservableCollection<Ticket>(tickets);
                SelectedTicket = Tickets.FirstOrDefault();

                StatusMessage = $"Loaded {Tickets.Count} tickets";
            }
            catch (UnauthorizedAccessException ex)
            {
                StatusMessage = "Access denied. Please log in again.";
                await HandleUnauthorizedAccess();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading tickets: {ex.Message}";
                Console.WriteLine($"Error in LoadDataAsync: {ex}");
                MessageBox.Show($"Failed to load tickets: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task HandleUnauthorizedAccess()
        {
            // Show message box on the UI thread
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                MessageBox.Show("Your session has expired. Please log in again.",
                    "Session Expired", MessageBoxButton.OK, MessageBoxImage.Warning);
            });

            // Perform logout (now properly awaited)
            await _authService.Logout();
            // This will trigger the logout flow in the application
        }

        private void ShowTicketsView()
        {
            IsTicketsViewActive = true;
            // Run data loading on a background thread to keep the UI responsive
            _ = Task.Run(async () =>
            {
                await Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    await LoadDataAsync();
                });
            });
        }

        private void ShowUsersView()
        {
            if (!IsAdmin)
            {
                StatusMessage = "Access denied. Admin privileges required.";
                return;
            }
            IsUsersViewActive = true;
            // Implementation for showing users view
        }

        private async Task CreateTicketAsync()
        {
            try
            {
                var newTicket = new Ticket
                {
                    Title = "New Ticket",
                    Description = "Please describe your issue...",
                    Status = TicketStatus.New,
                    Priority = TicketPriority.Medium,
                    CreatedByUserId = CurrentUser.Id,
                    CreatedAt = DateTime.Now
                };

                // Show ticket editor dialog
                // var result = await ShowTicketEditorDialog(newTicket);
                // if (result == true)
                // {
                //     await _ticketService.CreateTicketAsync(newTicket);
                //     await LoadDataAsync(forceRefresh: true);
                //     StatusMessage = "Ticket created successfully";
                // }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error creating ticket: {ex.Message}";
                Console.WriteLine($"Error in CreateTicketAsync: {ex}");
            }
        }

        private async Task UpdateTicketStatusAsync(TicketStatus newStatus)
        {
            if (SelectedTicket == null) return;

            try
            {
                SelectedTicket.Status = newStatus;
                await _ticketService.UpdateTicketAsync(SelectedTicket);
                StatusMessage = "Ticket status updated";
                await LoadDataAsync(forceRefresh: true);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error updating ticket: {ex.Message}";
                Console.WriteLine($"Error in UpdateTicketStatusAsync: {ex}");
            }
        }

        private Task AssignTicketAsync()
        {
            // TODO: Implement ticket assignment functionality
            // This is a placeholder for future implementation
            // For now, just return a completed task
            return Task.CompletedTask;

            // Implementation example (commented out):
            /*
            if (SelectedTicket == null) return Task.CompletedTask;

            try
            {
                // Show user selection dialog
                // var user = await ShowUserSelectionDialog();
                // if (user != null)
                // {
                //     SelectedTicket.AssignedTo = user.Id;
                //     await _ticketService.UpdateTicketAsync(SelectedTicket);
                //     StatusMessage = $"Ticket assigned to {user.Username}";
                //     await LoadDataAsync(forceRefresh: true);
                // }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error assigning ticket: {ex.Message}";
                Console.WriteLine($"Error in AssignTicketAsync: {ex}");
            }
            */
        }

        private async Task LogoutAsync()
        {
            await _authService.Logout();
            // The App class will handle the navigation back to login
        }
    }
}