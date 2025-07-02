using System.ComponentModel.DataAnnotations;
using OfficeTicketingTool.Models.Enums;

namespace OfficeTicketingTool.Models
{
    public class User
    {
        public int Id { get; set; }

        public required string Username { get; set; }

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public required string PasswordHash { get; set; }

        [MaxLength(50)]
        public string? Phone { get; set; }

        [MaxLength(100)]
        public string? Department { get; set; }

        public UserRole Role { get; set; } = UserRole.User;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        // Navigation properties
        public virtual ICollection<Ticket> CreatedTickets { get; set; } = [];
        public virtual ICollection<Ticket> AssignedTickets { get; set; } = [];
        public virtual ICollection<Comment> Comments { get; set; } = [];
    }
}