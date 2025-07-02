using OfficeTicketingTool.Models;
using OfficeTicketingTool.Models.Enums;

namespace OfficeTicketingTool.Services
{
    public interface ITicketService
    {
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
    }
}