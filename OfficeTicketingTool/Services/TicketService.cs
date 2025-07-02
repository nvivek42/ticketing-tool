using Microsoft.EntityFrameworkCore;
using OfficeTicketingTool.Data;
using OfficeTicketingTool.Models;
using OfficeTicketingTool.Models.Enums;

namespace OfficeTicketingTool.Services
{
    public class TicketService : ITicketService
    {
        private readonly TicketingDbContext _context;

        public TicketService(TicketingDbContext context)
        {
            _context = context;
        }

        public async Task<List<Ticket>> GetAllTicketsAsync()
        {
            return await _context.Tickets
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .Include(t => t.Category)
                .Include(t => t.Comments)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Ticket>> GetTicketsByStatusAsync(TicketStatus status)
        {
            return await _context.Tickets
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .Include(t => t.Category)
                .Where(t => t.Status == status)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Ticket>> GetTicketsByUserAsync(int userId)
        {
            return await _context.Tickets
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .Include(t => t.Category)
                .Where(t => t.CreatedByUserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Ticket>> GetAssignedTicketsAsync(int userId)
        {
            return await _context.Tickets
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .Include(t => t.Category)
                .Where(t => t.AssignedToUserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<Ticket?> GetTicketByIdAsync(int id)
        {
            return await _context.Tickets
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .Include(t => t.Category)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Ticket> CreateTicketAsync(Ticket ticket)
        {
            ticket.CreatedAt = DateTime.Now;
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            // Reload with navigation properties
            return await GetTicketByIdAsync(ticket.Id) ?? ticket;
        }

        public async Task<Ticket> UpdateTicketAsync(Ticket ticket, int id)
        {
            var existingTicket = await _context.Tickets.FindAsync(id);
            if (existingTicket == null)
            {
                throw new KeyNotFoundException($"Ticket with ID {id} not found.");
            }

            existingTicket.Title = ticket.Title;
            existingTicket.Description = ticket.Description;
            existingTicket.CategoryId = ticket.CategoryId;
            existingTicket.Status = ticket.Status;
            existingTicket.AssignedToUserId = ticket.AssignedToUserId;
            existingTicket.UpdatedAt = DateTime.Now;

            _context.Tickets.Update(existingTicket);
            await _context.SaveChangesAsync();

            // Reload with navigation properties
            return await GetTicketByIdAsync(id) ?? existingTicket;
        }
        public async Task<bool> DeleteTicketAsync(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null) return false;

            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssignTicketAsync(int ticketId, int userId)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null) return false;

            ticket.AssignedToUserId = userId;
            ticket.UpdatedAt = DateTime.Now;

            if (ticket.Status == TicketStatus.Open)
            {
                ticket.Status = TicketStatus.InProgress;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateTicketStatusAsync(int ticketId, TicketStatus status)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null) return false;

            ticket.Status = status;
            ticket.UpdatedAt = DateTime.Now;

            if (status == TicketStatus.Resolved || status == TicketStatus.Closed)
            {
                ticket.ResolvedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Ticket>> SearchTicketsAsync(string searchTerm)
        {
            return await _context.Tickets
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .Include(t => t.Category)
                .Where(t => t.Title.Contains(searchTerm) ||
                           t.Description.Contains(searchTerm) ||
                           t.CreatedByUser.FirstName.Contains(searchTerm) ||
                           t.CreatedByUser.LastName.Contains(searchTerm))
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

       
       
    }
}