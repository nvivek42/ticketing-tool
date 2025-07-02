using Pomelo.EntityFrameworkCore.MySql;
using Microsoft.EntityFrameworkCore; 
using Microsoft.EntityFrameworkCore.Design;
using OfficeTicketingTool.Models;
using OfficeTicketingTool.Models.Enums;

namespace OfficeTicketingTool.Data
{
    public class TicketingDbContext(DbContextOptions<TicketingDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = "Server=localhost;Database=TicketingToolDb;Uid=root;Pwd=root123;";
            var serverVersion = new MySqlServerVersion(new Version(8, 0, 30)); 
            optionsBuilder.UseMySql(connectionString, serverVersion);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired();
                entity.Property(e => e.Role).HasConversion<int>();
                entity.HasIndex(e => e.Email).IsUnique();

                entity.HasMany(u => u.CreatedTickets)
                      .WithOne(t => t.CreatedByUser)
                      .HasForeignKey(t => t.CreatedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(u => u.AssignedTickets)
                      .WithOne(t => t.AssignedToUser)
                      .HasForeignKey(t => t.AssignedToUserId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Category configuration
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // Ticket configuration
            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.Status).HasConversion<int>();
                entity.Property(e => e.Priority).HasConversion<int>();

                entity.HasOne(t => t.Category)
                      .WithMany(c => c.Tickets)
                      .HasForeignKey(t => t.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Comment configuration
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Content).IsRequired();

                entity.HasOne(c => c.Ticket)
                      .WithMany(t => t.Comments)
                      .HasForeignKey(c => c.TicketId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.User)
                      .WithMany(u => u.Comments)
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Seed data
            SeedData(modelBuilder);
        }

        // Updated seed data to include required 'Username' and 'PasswordHash' properties
        private static void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Technical Support", Description = "Technical issues and bugs" },
                new Category { Id = 2, Name = "Feature Request", Description = "New feature requests" },
                new Category { Id = 3, Name = "General Inquiry", Description = "General questions and inquiries" },
                new Category { Id = 4, Name = "Account Issues", Description = "Account related problems" }
            );

            // Seed Users
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    PasswordHash = "hashedpassword1", // Replace with actual hash
                    FirstName = "Admin",
                    LastName = "User",
                    Email = "admin@company.com",
                    Role = UserRole.Admin,
                    Department = "IT"
                },
                new User
                {
                    Id = 2,
                    Username = "john.agent",
                    PasswordHash = "hashedpassword2", // Replace with actual hash
                    FirstName = "John",
                    LastName = "Agent",
                    Email = "john.agent@company.com",
                    Role = UserRole.Agent,
                    Department = "Support"
                },
                new User
                {
                    Id = 3,
                    Username = "jane.doe",
                    PasswordHash = "hashedpassword3", // Replace with actual hash
                    FirstName = "Jane",
                    LastName = "Doe",
                    Email = "jane.doe@company.com",
                    Role = UserRole.User,
                    Department = "Sales"
                }
            );
        }    }
}