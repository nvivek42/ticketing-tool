using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OfficeTicketingTool.Data;
using OfficeTicketingTool.Models;
using OfficeTicketingTool.Models.Enums;

namespace OfficeTicketingTool.Services
{
    public class UserService : IUserService, IDisposable
    {
        private readonly TicketingDbContext _context;

        public UserService(TicketingDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users
                .AsNoTracking()
                .OrderBy(u => u.Username)
                .ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<List<User>> GetUsersByRoleAsync(UserRole role)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.Role == role)
                .ToListAsync();
        }
        public async Task<User> CreateUserAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrWhiteSpace(user.PasswordHash))
                throw new ArgumentException("Password is required", nameof(user.PasswordHash));

            if (await _context.Users.AnyAsync(u => u.Username == user.Username))
                throw new ApplicationException($"Username {user.Username} is already taken");

            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                throw new ApplicationException($"Email {user.Email} is already registered");

            user.Id = 0;
            user.CreatedAt = DateTime.UtcNow;
            user.IsActive = true;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var existingUser = await _context.Users.FindAsync(user.Id);
            if (existingUser == null)
                throw new KeyNotFoundException($"User with ID {user.Id} not found");

            // Prevent username and email changes through this method
            user.Username = existingUser.Username;
            user.Email = existingUser.Email;
            user.PasswordHash = existingUser.PasswordHash;
            user.CreatedAt = existingUser.CreatedAt;

            _context.Entry(existingUser).CurrentValues.SetValues(user);
            _context.Entry(existingUser).State = EntityState.Modified;

            await _context.SaveChangesAsync();
            return existingUser;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id.ToString());
            if (user == null)
                return false;

            // Soft delete by marking as inactive
            user.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ValidateUserCredentialsAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return false;

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

            if (user == null)
                return false;

            return VerifyPasswordHash(password, user.PasswordHash);
        }

        public async Task ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found");

            if (!VerifyPasswordHash(currentPassword, user.PasswordHash))
                throw new ApplicationException("Current password is incorrect");

            user.PasswordHash = HashPassword(newPassword);
            await _context.SaveChangesAsync();
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private bool VerifyPasswordHash(string password, string storedHash)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(storedHash))
                return false;

            var hashedInput = HashPassword(password);
            return string.Equals(hashedInput, storedHash, StringComparison.Ordinal);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
