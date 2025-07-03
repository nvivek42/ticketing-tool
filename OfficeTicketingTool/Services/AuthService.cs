using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OfficeTicketingTool.Data;
using OfficeTicketingTool.Models;
using OfficeTicketingTool.Utilities;
using System.Linq;

namespace OfficeTicketingTool.Services
{
    public class AuthService(TicketingDbContext context, IPasswordHasher passwordHasher) : IAuthService
    {
        private readonly TicketingDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
        private readonly IPasswordHasher _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        private User? _currentUser;

        public User CurrentUser => _currentUser;
        public bool IsAuthenticated => _currentUser != null;

        public void Logout()
        {
            _currentUser = null;
            Console.WriteLine("User logged out successfully");
        }

        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    Console.WriteLine($"Authentication failed: Username or password is empty. Username: {username}");
                    return null;
                }

                Console.WriteLine($"Attempting to authenticate user: {username}");
                
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == username);

                if (user == null)
                {
                    Console.WriteLine($"User not found: {username}");
                    return null;
                }

                if (!user.IsActive)
                {
                    Console.WriteLine($"User account is not active: {username}");
                    return null;
                }

                var isPasswordValid = _passwordHasher.VerifyPassword(user.PasswordHash, password);
                if (!isPasswordValid)
                {
                    Console.WriteLine($"Invalid password for user: {username}");
                    return null;
                }

                Console.WriteLine($"Authentication successful for user: {username}");
                _currentUser = user;
                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during authentication for user {username}: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            if (string.IsNullOrEmpty(newPassword))
                throw new ArgumentException("New password cannot be empty", nameof(newPassword));

            var user = await _context.Users.FindAsync(userId);
            if (user == null || !_passwordHasher.VerifyPassword(user.PasswordHash, currentPassword))
                return false;

            user.PasswordHash = _passwordHasher.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = _currentUser?.Id;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ResetPasswordAsync(int userId, string newPassword, int resetByUserId)
        {
            if (string.IsNullOrEmpty(newPassword))
                throw new ArgumentException("New password cannot be empty", nameof(newPassword));

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            user.PasswordHash = _passwordHasher.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = resetByUserId;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HasPermissionAsync(int userId, string permission)
        {
            if (string.IsNullOrEmpty(permission))
                return false;

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || user.Role == null)
                return false;

            // Assuming UserRole does not have a Permissions property, you need to adjust this logic.
            // Replace the following line with your actual permission-checking logic.
            return false; // Placeholder: Replace with actual permission-checking logic.
        }
    }
}
