using Microsoft.EntityFrameworkCore;
using backend.Models;
using System.Security.Cryptography;
using System.Text;

namespace backend.Data;

public static class DbSeeder
{
    public static void SeedDatabase(ApplicationDbContext context)
    {
        // Ensure database is created
        context.Database.EnsureCreated();

        // Check if data already exists
        if (context.Businesses.Any())
        {
            return; // Database already seeded
        }

        // Hash password helper
        string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        // Create Business entity (without OwnerId - will be set after owner is created)
        var business = new Business
        {
            Name = "Sample Beauty Salon",
            Description = "A full-service beauty salon offering hair, nails, and spa services",
            Address = "123 Main Street, Vilnius, Lithuania",
            ContactEmail = "contact@beautysalon.com",
            PhoneNumber = "+370 600 00000",
            CreatedAt = DateTime.UtcNow
        };

        // Temporarily disable FK constraint checking to allow Business insertion with OwnerId = 0
        context.Database.ExecuteSqlRaw("ALTER TABLE Businesses NOCHECK CONSTRAINT ALL");
        
        try
        {
            // Add Business to context and save to get its Id
            context.Businesses.Add(business);
            context.SaveChanges();

            // Create Owner/Admin User with correct BusinessId
            var owner = new User
            {
                BusinessId = business.Id,
                Name = "John Owner",
                PasswordHash = HashPassword("admin123"),
                Phone = "+370 600 00001",
                Role = "Admin",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            // Create Employee 1
            var employee1 = new User
            {
                BusinessId = business.Id,
                Name = "Jane Employee",
                PasswordHash = HashPassword("employee123"),
                Phone = "+370 600 00002",
                Role = "Employee",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            // Create Employee 2
            var employee2 = new User
            {
                BusinessId = business.Id,
                Name = "Bob Manager",
                PasswordHash = HashPassword("manager123"),
                Phone = "+370 600 00003",
                Role = "Manager",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            // Add all users and save to get owner.Id
            context.Users.AddRange(owner, employee1, employee2);
            context.SaveChanges();

            // Now update Business.OwnerId with the actual owner.Id
            business.OwnerId = owner.Id;
            context.Businesses.Update(business);
            context.SaveChanges();
        }
        finally
        {
            // Re-enable FK constraint checking
            context.Database.ExecuteSqlRaw("ALTER TABLE Businesses CHECK CONSTRAINT ALL");
        }

        // Create Products (5 products)
        var products = new List<Product>
        {
            new Product
            {
                BusinessId = business.Id,
                Name = "Hair Shampoo",
                Description = "Professional hair shampoo for all hair types",
                Price = 15.99m,
                Tags = new List<string> { "hair", "shampoo", "care" },
                Available = true,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                BusinessId = business.Id,
                Name = "Hair Conditioner",
                Description = "Deep conditioning treatment for healthy hair",
                Price = 18.99m,
                Tags = new List<string> { "hair", "conditioner", "care" },
                Available = true,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                BusinessId = business.Id,
                Name = "Nail Polish - Red",
                Description = "Long-lasting red nail polish",
                Price = 8.99m,
                Tags = new List<string> { "nail", "polish", "red" },
                Available = true,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                BusinessId = business.Id,
                Name = "Face Mask",
                Description = "Hydrating face mask for all skin types",
                Price = 25.99m,
                Tags = new List<string> { "face", "mask", "skincare" },
                Available = true,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                BusinessId = business.Id,
                Name = "Hair Styling Gel",
                Description = "Strong hold styling gel for men and women",
                Price = 12.99m,
                Tags = new List<string> { "hair", "styling", "gel" },
                Available = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Products.AddRange(products);
        context.SaveChanges();

        // Create Services (3 services)
        var services = new List<Service>
        {
            new Service
            {
                BusinessId = business.Id,
                Name = "Haircut",
                Description = "Professional haircut and styling",
                Price = 35.00m,
                DurationMinutes = 60,
                Available = true,
                CreatedAt = DateTime.UtcNow
            },
            new Service
            {
                BusinessId = business.Id,
                Name = "Manicure",
                Description = "Full manicure with nail polish",
                Price = 25.00m,
                DurationMinutes = 45,
                Available = true,
                CreatedAt = DateTime.UtcNow
            },
            new Service
            {
                BusinessId = business.Id,
                Name = "Facial Treatment",
                Description = "Deep cleansing facial treatment",
                Price = 55.00m,
                DurationMinutes = 90,
                Available = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Services.AddRange(services);
        context.SaveChanges();
    }
}
