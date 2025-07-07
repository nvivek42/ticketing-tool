using Microsoft.EntityFrameworkCore;
using OfficeTicketingTool.Data;
using OfficeTicketingTool.Models;
using OfficeTicketingTool.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OfficeTicketingTool.Services
{
    public class TicketService : ITicketService
    {
        private readonly TicketingDbContext _context;

        public TicketService(TicketingDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<Ticket>> GetAllTicketsAsync()
        {
            return await _context.Tickets
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .Include(t => t.Category)
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

        public async Task<IEnumerable<Ticket>> GetTicketsForCurrentUserAsync(User currentUser)
        {
            var filter = new TicketFilter();
            
            switch (currentUser.Role)
            {
                case UserRole.Admin:
                    filter.IncludeAll = true;
                    break;
                case UserRole.Agent:
                    filter.AssignedTo = currentUser.Id;
                    filter.IncludeAll = true; // Agents can see all tickets assigned to them
                    break;
                default: // Regular user
                    filter.CreatedBy = currentUser.Id;
                    break;
            }

            return await GetTicketsAsync(filter);
        }

        public async Task<IEnumerable<Ticket>> GetTicketsAsync(TicketFilter filter)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            IQueryable<Ticket> query = _context.Tickets
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .Include(t => t.Category)
                .AsQueryable();

            // Apply filters
            if (!filter.IncludeAll)
            {
                if (filter.CreatedBy.HasValue)
                    query = query.Where(t => t.CreatedByUserId == filter.CreatedBy);
                
                if (filter.AssignedTo.HasValue)
                    query = query.Where(t => t.AssignedToUserId == filter.AssignedTo);
            }

            if (filter.Status.HasValue)
                query = query.Where(t => t.Status == filter.Status);

            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.ToLower();
                query = query.Where(t => 
                    t.Title.ToLower().Contains(searchTerm) || 
                    t.Description.ToLower().Contains(searchTerm));
            }

            if (filter.FromDate.HasValue)
                query = query.Where(t => t.CreatedAt >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(t => t.CreatedAt <= filter.ToDate.Value);

            if (filter.CategoryId.HasValue)
                query = query.Where(t => t.CategoryId == filter.CategoryId);

            return await query
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<PagedResult<Ticket>> GetTicketsPagedAsync(TicketFilter filter, int pageNumber, int pageSize)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            IQueryable<Ticket> query = _context.Tickets
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .Include(t => t.Category)
                .AsQueryable();

            // Apply filters (same as GetTicketsAsync)
            if (!filter.IncludeAll)
            {
                if (filter.CreatedBy.HasValue)
                    query = query.Where(t => t.CreatedByUserId == filter.CreatedBy);
                
                if (filter.AssignedTo.HasValue)
                    query = query.Where(t => t.AssignedToUserId == filter.AssignedTo);
            }

            if (filter.Status.HasValue)
                query = query.Where(t => t.Status == filter.Status);

            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.ToLower();
                query = query.Where(t => 
                    t.Title.ToLower().Contains(searchTerm) || 
                    t.Description.ToLower().Contains(searchTerm));
            }

            if (filter.FromDate.HasValue)
                query = query.Where(t => t.CreatedAt >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(t => t.CreatedAt <= filter.ToDate.Value);

            if (filter.CategoryId.HasValue)
                query = query.Where(t => t.CategoryId == filter.CategoryId);

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Ticket>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<Ticket> CreateTicketAsync(Ticket ticket)
        {
            if (ticket == null)
                throw new ArgumentNullException(nameof(ticket));

            ticket.CreatedAt = DateTime.UtcNow;
            ticket.UpdatedAt = DateTime.UtcNow;

            await _context.Tickets.AddAsync(ticket);
            await _context.SaveChangesAsync();

            return ticket;
        }

        public async Task<Ticket> UpdateTicketAsync(Ticket ticket, int id)
        {
            if (ticket == null)
                throw new ArgumentNullException(nameof(ticket));

            var existingTicket = await _context.Tickets.FindAsync(id);
            if (existingTicket == null)
                throw new KeyNotFoundException($"Ticket with ID {id} not found.");

            // Update only the allowed fields
            existingTicket.Title = ticket.Title;
            existingTicket.Description = ticket.Description;
            existingTicket.Status = ticket.Status;
            existingTicket.Priority = ticket.Priority;
            existingTicket.AssignedToUserId = ticket.AssignedToUserId;
            existingTicket.CategoryId = ticket.CategoryId;
            existingTicket.UpdatedAt = DateTime.UtcNow;

            _context.Tickets.Update(existingTicket);
            await _context.SaveChangesAsync();
            return existingTicket;
        }

        public async Task UpdateTicketAsync(Ticket ticket)
        {
            if (ticket == null)
                throw new ArgumentNullException(nameof(ticket));

            var existingTicket = await _context.Tickets.FindAsync(ticket.Id);
            if (existingTicket == null)
                throw new KeyNotFoundException($"Ticket with ID {ticket.Id} not found.");

            // Update only the allowed fields
            existingTicket.Title = ticket.Title;
            existingTicket.Description = ticket.Description;
            existingTicket.Status = ticket.Status;
            existingTicket.Priority = ticket.Priority;
            existingTicket.AssignedToUserId = ticket.AssignedToUserId;
            existingTicket.CategoryId = ticket.CategoryId;
            existingTicket.UpdatedAt = DateTime.UtcNow;

            _context.Tickets.Update(existingTicket);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Ticket>> SearchTicketsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<Ticket>();

            var term = searchTerm.ToLower().Trim();
            
            return await _context.Tickets
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .Include(t => t.Category)
                .Where(t => t.Title.ToLower().Contains(term) || 
                           t.Description.ToLower().Contains(term) ||
                           t.Comments.Any(c => c.Content.ToLower().Contains(term)))
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> UpdateTicketStatusAsync(int ticketId, TicketStatus status)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null)
                return false;

            ticket.Status = status;
            ticket.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssignTicketAsync(int ticketId, int userId)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null)
                return false;

            // Verify the user exists and is an agent
            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.Role != UserRole.Agent)
                return false;

            ticket.AssignedToUserId = userId;
            ticket.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTicketAsync(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
                return false;

            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();
            return true;
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
    }
}