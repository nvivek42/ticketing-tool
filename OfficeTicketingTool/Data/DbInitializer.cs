using System;
using System.Linq;
using OfficeTicketingTool.Models;
using OfficeTicketingTool.Models.Enums;
using OfficeTicketingTool.Utilities;

namespace OfficeTicketingTool.Data
{
  
   
    namespace OfficeTicketingTool.Data
    {
        public static class DbInitializer
        {
            public static void Initialize(TicketingDbContext context, IPasswordHasher passwordHasher)
            {
                context.Database.EnsureCreated();

                // Check if database is already seeded
                if (context.Users.Any())
                {
                    return; // DB has been seeded
                }

                // Add default categories
                var categories = new Category[]
                {
                new() { Name = "Hardware", Description = "Hardware related issues" },
                new() { Name = "Software", Description = "Software installation and issues" },
                new () { Name = "Network", Description = "Network and connectivity issues" },
                new () { Name = "Email", Description = "Email and communication issues" },
                new () { Name = "Account", Description = "User account and access issues" }
                };

                context.Categories.AddRange(categories);
                context.SaveChanges();

                // Add default admin user
                var admin = new User
                {
                    Username = "admin",
                    FirstName = "Admin",
                    LastName = "User",
                    Email = "admin@office.com",
                    PasswordHash = passwordHasher.HashPassword("admin123"),
                    Role = UserRole.Admin,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = 1 // System user
                };

                // Add default support agent
                var agent = new User
                {
                    Username = "agent1",
                    FirstName = "Support",
                    LastName = "Agent",
                    Email = "agent@office.com",
                    PasswordHash = passwordHasher.HashPassword("agent123"),
                    Role = UserRole.Agent,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = 1 // System user
                };

                // Add default regular user
                var user = new User
                {
                    Username = "user1",
                    FirstName = "Regular",
                    LastName = "User",
                    Email = "user@office.com",
                    PasswordHash = passwordHasher.HashPassword("user123"),
                    Role = UserRole.User,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = 1 // System user
                };

                context.Users.AddRange(admin, agent, user);
                context.SaveChanges();
            }
        }
    }
}
