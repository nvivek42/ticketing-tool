using System.Collections.ObjectModel;
using System.Windows.Input;
using OfficeTicketingTool.Models;
using OfficeTicketingTool.Models.Enums;

namespace OfficeTicketingTool.ViewModels
{
    public class UserViewModel : BaseViewModel
    {
        private string _username;
        private string _email;
        private string _firstName;
        private string _lastName;
        private UserRole _role;
        private bool _isActive;
        private ObservableCollection<User> _users;
        private User _selectedUser;

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string FirstName
        {
            get => _firstName;
            set => SetProperty(ref _firstName, value);
        }

        public string LastName
        {
            get => _lastName;
            set => SetProperty(ref _lastName, value);
        }

        public UserRole Role
        {
            get => _role;
            set => SetProperty(ref _role, value);
        }

        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        public ObservableCollection<User> Users
        {
            get => _users ??= new ObservableCollection<User>();
            set => SetProperty(ref _users, value);
        }

        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (SetProperty(ref _selectedUser, value) && value != null)
                {
                    LoadUserData(value);
                }
            }
        }

        public ICommand AddUserCommand { get; }
        public ICommand UpdateUserCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand ClearFormCommand { get; }

        private void LoadUserData(User user)
        {
            Username = user.Username;
            Email = user.Email;
            FirstName = user.FirstName;
            LastName = user.LastName;
            Role = user.Role;
            IsActive = user.IsActive;
        }
    }
}
