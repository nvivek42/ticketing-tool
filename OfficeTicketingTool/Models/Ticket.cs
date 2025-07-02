using System.ComponentModel.DataAnnotations;
using OfficeTicketingTool.Models.Enums;

namespace OfficeTicketingTool.Models
{
    public class Ticket
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public TicketStatus Status { get; set; } = TicketStatus.Open;

        public TicketPriority Priority { get; set; } = TicketPriority.Medium;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? ResolvedAt { get; set; }

        public DateTime? DueDate { get; set; }

        // Foreign Keys
        public int CreatedByUserId { get; set; }
        public int? AssignedToUserId { get; set; }
        public int CategoryId { get; set; }

        // Navigation properties
        public virtual User CreatedByUser { get; set; } = null!;
        public virtual User? AssignedToUser { get; set; }
        public virtual Category Category { get; set; } = null!;
        public virtual ICollection<Comment> Comments { get; set; } = [];

        // Computed properties
        public string StatusDisplay => Status.ToString().Replace("_", " ");
        public string PriorityDisplay => Priority.ToString();
        public bool IsOverdue => DueDate.HasValue && DueDate.Value < DateTime.Now && Status != TicketStatus.Closed && Status != TicketStatus.Resolved;
    }
}