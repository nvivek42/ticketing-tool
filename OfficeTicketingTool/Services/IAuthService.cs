using OfficeTicketingTool.Models;
using System.Threading.Tasks;

namespace OfficeTicketingTool.Services
{
    public interface IAuthService
    {
        /// <summary>
        /// Authenticates a user with the provided credentials
        /// </summary>
        Task<User> AuthenticateAsync(string username, string password);
        
        /// <summary>
        /// Changes the password for the specified user after verifying the current password
        /// </summary>
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        
        /// <summary>
        /// Resets a user's password without requiring the current password (admin function)
        /// </summary>
        Task<bool> ResetPasswordAsync(int userId, string newPassword, int resetByUserId);
        
        /// <summary>
        /// Logs out the current user
        /// </summary>
        void Logout();
        
        /// <summary>
        /// Gets the currently authenticated user
        /// </summary>
        User CurrentUser { get; }
        
        /// <summary>
        /// Gets a value indicating whether a user is currently authenticated
        /// </summary>
        bool IsAuthenticated { get; }
        
        /// <summary>
        /// Checks if a user has a specific permission
        /// </summary>
        Task<bool> HasPermissionAsync(int userId, string permission);
    }
}
