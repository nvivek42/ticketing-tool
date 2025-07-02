using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.VisualBasic;
using System.Collections.ObjectModel;
using System.Windows;
using OfficeTicketingTool.Models;
using OfficeTicketingTool.Models.Enums;
using OfficeTicketingTool.Services;

namespace OfficeTicketingTool.ViewModels
{
    public partial class TicketViewModel : BaseViewModel
    {
        private readonly ITicketService _ticketService;
        private readonly IUserService _userService;
        private readonly ICategoryService _categoryService;

        [ObservableProperty]
        private ObservableCollection<Ticket> tickets = [];

        [ObservableProperty]
        private ObservableCollection<User> users = [];

        [ObservableProperty]
        private ObservableCollection<Category> categories = [];

        [ObservableProperty]
        private Ticket? selectedTicket;

        [ObservableProperty]
        private string ticketTitle = string.Empty;

        [ObservableProperty]
        private string ticketDescription = string.Empty;

        [ObservableProperty]
        private TicketPriority selectedPriority = TicketPriority.Medium;

        [ObservableProperty]
        private Category? selectedCategory;

        [ObservableProperty]
        private User? selectedAssignee;

        [ObservableProperty]
        private DateTime? dueDate;

        [ObservableProperty]
        private string searchText = string.Empty;

        [ObservableProperty]
        private TicketStatus? statusFilter;

        public TicketViewModel(ITicketService ticketService, IUserService userService, ICategoryService categoryService)
        {
            _ticketService = ticketService;
            _userService = userService;
            _categoryService = categoryService;
            Title = "Ticket Management";
        }

        [RelayCommand]
        private async Task LoadDataAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                StatusMessage = "Loading tickets...";

                var ticketTask = _ticketService.GetAllTicketsAsync();
                var userTask = _userService.GetAllUsersAsync();
                var categoryTask = _categoryService.GetAllCategoriesAsync();

                await Task.WhenAll(ticketTask, userTask, categoryTask);

                Tickets.Clear();
                Users.Clear();
                Categories.Clear();

                foreach (var ticket in await ticketTask)
                    Tickets.Add(ticket);

                foreach (var user in await userTask)
                    Users.Add(user);

                foreach (var category in await categoryTask)
                    Categories.Add(category);

                StatusMessage = $"Loaded {Tickets.Count} tickets";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading data: {ex.Message}";
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task CreateTicketAsync()
        {
            if (string.IsNullOrWhiteSpace(TicketTitle) ||
                string.IsNullOrWhiteSpace(TicketDescription) ||
                SelectedCategory == null)
            {
                MessageBox.Show("Please fill in all required fields (Title, Description, Category).",
                               "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsBusy = true;
                StatusMessage = "Creating ticket...";

                var newTicket = new Ticket
                {
                    Title = TicketTitle,
                    Description = TicketDescription,
                    Priority = SelectedPriority,
                    CategoryId = SelectedCategory.Id,
                    CreatedByUserId = 1, // TODO: Get from logged in user
                    AssignedToUserId = SelectedAssignee?.Id,
                    DueDate = DueDate
                };

                var createdTicket = await _ticketService.CreateTicketAsync(newTicket);
                Tickets.Insert(0, createdTicket);
                ClearForm();
                StatusMessage = "Ticket created successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error creating ticket: {ex.Message}";
                MessageBox.Show($"Error creating ticket: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task UpdateTicketStatusAsync(object parameter)
        {
            if (SelectedTicket == null || parameter is not TicketStatus newStatus) return;

            try
            {
                IsBusy = true;
                StatusMessage = "Updating ticket status...";

                var success = await _ticketService.UpdateTicketStatusAsync(SelectedTicket.Id, newStatus);
                if (success)
                {
                    SelectedTicket.Status = newStatus;
                    SelectedTicket.UpdatedAt = DateTime.Now;
                    StatusMessage = $"Ticket status updated to {newStatus}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error updating status: {ex.Message}";
                MessageBox.Show($"Error updating status: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task AssignTicketAsync()
        {
            if (SelectedTicket == null || SelectedAssignee == null) return;

            try
            {
                IsBusy = true;
                StatusMessage = "Assigning ticket...";

                var success = await _ticketService.AssignTicketAsync(SelectedTicket.Id, SelectedAssignee.Id);
                if (success)
                {
                    SelectedTicket.AssignedToUserId = SelectedAssignee.Id;
                    SelectedTicket.AssignedToUser = SelectedAssignee;
                    StatusMessage = $"Ticket assigned to {SelectedAssignee.FullName}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error assigning ticket: {ex.Message}";
                MessageBox.Show($"Error assigning ticket: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task SearchTicketsAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                await LoadDataAsync();
                return;
            }

            try
            {
                IsBusy = true;
                StatusMessage = "Searching tickets...";

                var searchResults = await _ticketService.SearchTicketsAsync(SearchText);
                Tickets.Clear();

                foreach (var ticket in searchResults)
                    Tickets.Add(ticket);

                StatusMessage = $"Found {Tickets.Count} tickets matching '{SearchText}'";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error searching: {ex.Message}";
                MessageBox.Show($"Error searching: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task FilterByStatusAsync()
        {
            if (StatusFilter == null)
            {
                await LoadDataAsync();
                return;
            }

            try
            {
                IsBusy = true;
                StatusMessage = $"Filtering by {StatusFilter}...";

                var filteredTickets = await _ticketService.GetTicketsByStatusAsync(StatusFilter.Value);
                Tickets.Clear();

                foreach (var ticket in filteredTickets)
                    Tickets.Add(ticket);

                StatusMessage = $"Showing {Tickets.Count} {StatusFilter} tickets";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error filtering: {ex.Message}";
                MessageBox.Show($"Error filtering: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task DeleteTicketAsync(Ticket? ticket)
        {
            if (ticket == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete ticket '{ticket.Title}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsBusy = true;
                    StatusMessage = "Deleting ticket...";

                    var success = await _ticketService.DeleteTicketAsync(ticket.Id);
                    if (success)
                    {
                        Tickets.Remove(ticket);
                        StatusMessage = "Ticket deleted successfully";
                    }
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error deleting ticket: {ex.Message}";
                    MessageBox.Show($"Error deleting ticket: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        private void ClearForm()
        {
            TicketTitle = string.Empty;
            TicketDescription = string.Empty;
            SelectedPriority = TicketPriority.Medium;
            SelectedCategory = null;
            SelectedAssignee = null;
            DueDate = null;
        }

        // Properties for binding enums to ComboBoxes
        public static Array PriorityOptions => Enum.GetValues(typeof(TicketPriority));
        public static Array StatusOptions => Enum.GetValues(typeof(TicketStatus));
    }
}