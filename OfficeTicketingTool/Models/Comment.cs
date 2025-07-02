using System.ComponentModel.DataAnnotations;

namespace OfficeTicketingTool.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsInternal { get; set; } = false;

        // Foreign Keys
        public int TicketId { get; set; }
        public int UserId { get; set; }

        // Navigation properties
        public virtual Ticket Ticket { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}