using Microsoft.EntityFrameworkCore;
using REAK.API.Models.Entities;
using REAK.API.Models.Enums;
using BCrypt.Net;

namespace REAK.API.Data;

public static class DatabaseSeeder
{
    public static async System.Threading.Tasks.Task SeedAsync(ReakDbContext context)
    {
        // Ensure database is created
        await context.Database.MigrateAsync();

        // Check if SuperAdmin already exists
        var superAdminExists = await context.Users
            .AnyAsync(u => u.Role == UserRole.SuperAdmin);

        if (!superAdminExists)
        {
            // Create SuperAdmin user
            var superAdmin = new User
            {
                Email = "admin@reak.com",
                // Password: Admin@123 (hashed with BCrypt)
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = UserRole.SuperAdmin,
                FullName = "System Administrator",
                Phone = "+1234567890",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(superAdmin);
            await context.SaveChangesAsync();

            Console.WriteLine("==============================================");
            Console.WriteLine("SuperAdmin user created successfully!");
            Console.WriteLine("==============================================");
            Console.WriteLine($"Email: {superAdmin.Email}");
            Console.WriteLine("Password: Admin@123");
            Console.WriteLine("==============================================");
            Console.WriteLine("IMPORTANT: Please change the password after first login.");
            Console.WriteLine("==============================================");
        }
    }
}
