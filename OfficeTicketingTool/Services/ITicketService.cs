using OfficeTicketingTool.Models;
using OfficeTicketingTool.Models.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OfficeTicketingTool.Services
{
    public interface ITicketService
    {
        // Existing methods
        Task<List<Ticket>> GetAllTicketsAsync();
        Task<List<Ticket>> GetTicketsByStatusAsync(TicketStatus status);
        Task<List<Ticket>> GetTicketsByUserAsync(int userId);
        Task<List<Ticket>> GetAssignedTicketsAsync(int userId);
        Task<Ticket?> GetTicketByIdAsync(int id);
        Task<Ticket> CreateTicketAsync(Ticket ticket);
        Task<Ticket> UpdateTicketAsync(Ticket ticket, int id);
        Task<bool> DeleteTicketAsync(int id);
        Task<bool> AssignTicketAsync(int ticketId, int userId);
        Task<bool> UpdateTicketStatusAsync(int ticketId, TicketStatus status);
        Task<List<Ticket>> SearchTicketsAsync(string searchTerm);
        
        // Methods from the duplicate definition
        Task<IEnumerable<Ticket>> GetTicketsForCurrentUserAsync(User currentUser);
        Task UpdateTicketAsync(Ticket ticket);
        Task<IEnumerable<Ticket>> GetTicketsAsync(TicketFilter filter);
        Task<PagedResult<Ticket>> GetTicketsPagedAsync(TicketFilter filter, int pageNumber, int pageSize);
    }
}