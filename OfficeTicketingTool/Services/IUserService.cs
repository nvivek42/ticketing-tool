using OfficeTicketingTool.Models;
using OfficeTicketingTool.Models.Enums;

namespace OfficeTicketingTool.Services
{
    public interface IUserService
    {
        Task<List<User>> GetAllUsersAsync();
        Task<List<User>> GetUsersByRoleAsync(UserRole role);
        Task<User?> GetUserByIdAsync(int id);
        Task<User> CreateUserAsync(User user);
        Task<User> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int id);
    }
}