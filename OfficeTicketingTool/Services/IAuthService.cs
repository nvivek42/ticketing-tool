using OfficeTicketingTool.Models;
using System.Threading.Tasks;

namespace OfficeTicketingTool.Services
{
    public interface IAuthService
    {
        User CurrentUser { get; }
        bool IsAuthenticated { get; }
        
        Task<User?> AuthenticateAsync(string username, string password);
        Task<User> RegisterAsync(string username, string password, string firstName, string lastName, string email);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<bool> ResetPasswordAsync(int userId, string newPassword, int resetByUserId);
        Task<bool> HasPermissionAsync(int userId, string permission);
        Task<bool> Logout();
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}