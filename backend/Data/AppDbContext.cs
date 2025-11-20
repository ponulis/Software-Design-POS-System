using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

        public DbSet<Business> Businesses => Set<Business>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<GiftCard> GiftCards => Set<GiftCard>();
    public DbSet<Discount> Discounts => Set<Discount>();
    public DbSet<Tax> Tax => Set<Tax>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Relationships are not mentioned in the pdf file, adding common ones
        // one Business has many Employees
        modelBuilder.Entity<Business>()
            .HasMany(b => b.Employees)
            .WithOne(e => e.Business)
            .HasForeignKey(e => e.BusinessId);

        // One Business has many Products
        modelBuilder.Entity<Business>()
            .HasMany(b => b.Products)
            .WithOne(p => p.Business)
            .HasForeignKey(p => p.BusinessId);

        // One Order has many Items
        modelBuilder.Entity<Order>()
            .HasMany(o => o.OrderItems)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId);
        
        // One Order has many Payments (for split etc)
        modelBuilder.Entity<Order>()
            .HasMany(o => o.Payments)
            .WithOne(p => p.Order)
            .HasForeignKey(p => p.OrderId);

        // One Employee has many Orders
        modelBuilder.Entity<Employee>()
            .HasMany(e => e.Orders)
            .WithOne(o => o.Employee)
            .HasForeignKey(o => o.EmployeeId);

        // Appointment relationships
        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Employee)
            .WithMany(e => e.Appointments)
            .HasForeignKey(a => a.EmployeeId);

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Service)
            .WithMany(s => s.Appointments)
            .HasForeignKey(a => a.ServiceId);
        
        modelBuilder.Entity<Service>()
            .HasOne(s => s.Business)
            .WithMany(b => b.Services)
            .HasForeignKey(s => s.BusinessId);
        
        modelBuilder.Entity<Service>()
            .HasOne(s => s.Tax)
            .WithMany(t => t.Services)
            .HasForeignKey(s => s.TaxId);
        
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Tax)
            .WithMany(t => t.Products)
            .HasForeignKey(p => p.TaxId);
        
        modelBuilder.Entity<Tax>()
            .HasOne(t => t.Business)
            .WithMany(b => b.Taxes)
            .HasForeignKey(t => t.BusinessId);
        
        modelBuilder.Entity<GiftCard>()
            .HasOne(g => g.Payment)
            .WithMany(p => p.GiftCards)
            .HasForeignKey(g => g.PaymentId);
    }
}