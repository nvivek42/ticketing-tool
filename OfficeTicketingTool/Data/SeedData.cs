using System;
using System.Linq;
using System.Threading.Tasks;
using OfficeTicketingTool.Models;
using OfficeTicketingTool.Models.Enums;

namespace OfficeTicketingTool.Data
{
    public static class SeedData
    {
        public static void Initialize(TicketingDbContext context)
        {
            if (context.Users.Any())
            {
                return; // DB has been seeded
            }

            // Add sample categories
            var categories = new[]
            {
                    new Category { Name = "Hardware", Description = "Hardware related issues" },
                    new Category { Name = "Software", Description = "Software related issues" },
                    new Category { Name = "Network", Description = "Network related issues" },
                    new Category { Name = "Account", Description = "User account issues" },
                    new Category { Name = "Other", Description = "Other issues" }
                };
            context.Categories.AddRange(categories);

            // Add sample users
            var users = new[]
            {
                    new User
                    {
                        Id = 1, // Changed from Guid.NewGuid().ToString() to an integer value
                        Username = "admin",
                        Email = "admin@example.com",
                        FirstName = "Admin",
                        LastName = "User",
                        Role = UserRole.Admin,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        PasswordHash = HashPassword("admin123")
                    },
                    new User
                    {
                        Id = 2, // Changed from Guid.NewGuid().ToString() to an integer value
                        Username = "agent1",
                        Email = "agent1@example.com",
                        FirstName = "Support",
                        LastName = "Agent",
                        Role = UserRole.Agent,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        PasswordHash = HashPassword("agent123")
                    },
                    new User
                    {
                        Id = 3, // Changed from Guid.NewGuid().ToString() to an integer value
                        Username = "user1",
                        Email = "user1@example.com",
                        FirstName = "Regular",
                        LastName = "User",
                        Role = UserRole.User,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        PasswordHash = HashPassword("user123")
                    }
                };
            context.Users.AddRange(users);

            context.SaveChanges();

            // Add sample tickets
            var tickets = new[]
            {
                    new Ticket
                    {
                        Title = "Printer not working",
                        Description = "The office printer is showing an error message and won't print.",
                        Status = TicketStatus.Open,
                        Priority = TicketPriority.Medium,
                        CreatedAt = DateTime.UtcNow.AddDays(-2),
                        UpdatedAt = DateTime.UtcNow.AddDays(-1),
                        Category = categories[0], // Hardware
                        CreatedByUser = users[2], // Regular user
                        AssignedToUser = users[1] // Support agent
                    },
                    new Ticket
                    {
                        Title = "Email configuration",
                        Description = "Need help setting up email on my new laptop.",
                        Status = TicketStatus.InProgress,
                        Priority = TicketPriority.High,
                        CreatedAt = DateTime.UtcNow.AddDays(-1),
                        UpdatedAt = DateTime.UtcNow,
                        Category = categories[1], // Software
                        CreatedByUser = users[2], // Regular user
                        AssignedToUser = users[1] // Support agent
                    }
                };
            context.Tickets.AddRange(tickets);

            context.SaveChanges();

            // Add sample comments
            var comments = new[]
            {
                    new Comment
                    {
                        Content = "I've restarted the printer but the issue persists.",
                        CreatedAt = DateTime.UtcNow.AddHours(-12),
                        Ticket = tickets[0],
                        User = users[2] // Regular user
                    },
                    new Comment
                    {
                        Content = "I'll take a look at the printer in the morning.",
                        CreatedAt = DateTime.UtcNow.AddHours(-2),
                        Ticket = tickets[0],
                        User = users[1] // Support agent
                    },
                    new Comment
                    {
                        Content = "Thanks for the quick response!",
                        CreatedAt = DateTime.UtcNow.AddHours(-1),
                        Ticket = tickets[0],
                        User = users[2] // Regular user
                    }
                };
            context.Comments.AddRange(comments);

            context.SaveChanges();
        }

        private static string HashPassword(string password)
        {
            // This is a simple hash for demo purposes
            // In production, use a proper password hashing algorithm like BCrypt or ASP.NET Core Identity
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}
