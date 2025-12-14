using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Business> Businesses { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<Tax> Taxes { get; set; }
    public DbSet<Discount> Discounts { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<GiftCard> GiftCards { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Business configuration
        modelBuilder.Entity<Business>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Address).IsRequired().HasMaxLength(500);
            entity.Property(e => e.ContactEmail).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(50);

            // Relationship: Business -> Owner (User)
            // Owner is a User, but we use a simple foreign key relationship
            entity.HasOne(e => e.Owner)
                .WithMany()
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Phone).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50);

            // Relationship: User -> Business
            entity.HasOne(e => e.Business)
                .WithMany(b => b.Employees)
                .HasForeignKey(e => e.BusinessId)
                .OnDelete(DeleteBehavior.Restrict);

            // Index for business lookup
            entity.HasIndex(e => e.BusinessId);
            
            // Composite index for filtering users by BusinessId and Role
            entity.HasIndex(e => new { e.BusinessId, e.Role });
        });

        // Product configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");

            // Store Tags as comma-separated string
            entity.Property(e => e.Tags)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v == null ? new List<string>() : v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                    new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<string>>(
                        (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList()
                    )
                );

            // Relationship: Product -> Business
            entity.HasOne(e => e.Business)
                .WithMany(b => b.Products)
                .HasForeignKey(e => e.BusinessId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.BusinessId);
            
            // Composite index for filtering products by BusinessId and Available status
            entity.HasIndex(e => new { e.BusinessId, e.Available });
        });

        // Service configuration
        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");

            // Relationship: Service -> Business
            entity.HasOne(e => e.Business)
                .WithMany(b => b.Services)
                .HasForeignKey(e => e.BusinessId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.BusinessId);
        });

        // Order configuration
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SubTotal).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Discount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Tax).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Status).HasConversion<string>();

            // Relationship: Order -> Business
            entity.HasOne(e => e.Business)
                .WithMany(b => b.Orders)
                .HasForeignKey(e => e.BusinessId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relationship: Order -> Creator (User)
            entity.HasOne(e => e.Creator)
                .WithMany(u => u.OrdersCreated)
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.BusinessId);
            entity.HasIndex(e => e.CreatedBy);
            entity.HasIndex(e => e.Status);
            
            // Composite index for common query pattern: Orders by BusinessId and Status
            entity.HasIndex(e => new { e.BusinessId, e.Status });
        });

        // OrderItem configuration
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");

            // Relationship: OrderItem -> Order
            entity.HasOne(e => e.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relationship: OrderItem -> Product
            entity.HasOne(e => e.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(e => e.MenuId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.MenuId);
        });

        // Appointment configuration
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CustomerName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.CustomerPhone).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).HasConversion<string>();

            // Relationship: Appointment -> Business
            entity.HasOne(e => e.Business)
                .WithMany(b => b.Appointments)
                .HasForeignKey(e => e.BusinessId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relationship: Appointment -> Service (optional)
            entity.HasOne(e => e.Service)
                .WithMany(s => s.Appointments)
                .HasForeignKey(e => e.ServiceId)
                .OnDelete(DeleteBehavior.SetNull);

            // Relationship: Appointment -> Employee (optional)
            entity.HasOne(e => e.Employee)
                .WithMany(u => u.Appointments)
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);

            // Relationship: Appointment -> Order (optional, for prepaid appointments)
            entity.HasOne(e => e.Order)
                .WithOne(o => o.Appointment)
                .HasForeignKey<Appointment>(e => e.OrderId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.BusinessId);
            entity.HasIndex(e => e.Date);
            entity.HasIndex(e => e.Status);
            
            // Composite index for common query patterns:
            // - Appointments by Date and BusinessId (for date range queries)
            // - Appointments by EmployeeId and BusinessId (for employee schedules)
            entity.HasIndex(e => new { e.BusinessId, e.Date });
            entity.HasIndex(e => new { e.BusinessId, e.EmployeeId });
            entity.HasIndex(e => new { e.Date, e.EmployeeId, e.BusinessId });
        });

        // Tax configuration
        modelBuilder.Entity<Tax>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Rate).HasColumnType("decimal(5,2)");

            // Relationship: Tax -> Business
            entity.HasOne(e => e.Business)
                .WithMany(b => b.Taxes)
                .HasForeignKey(e => e.BusinessId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.BusinessId);
        });

        // Discount configuration
        modelBuilder.Entity<Discount>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Value).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Type).HasConversion<string>();

            // Relationship: Discount -> Business
            entity.HasOne(e => e.Business)
                .WithMany(b => b.Discounts)
                .HasForeignKey(e => e.BusinessId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.BusinessId);
        });

        // Payment configuration
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Method).HasConversion<string>();
            entity.Property(e => e.TransactionId).HasMaxLength(255);
            entity.Property(e => e.AuthorizationCode).HasMaxLength(100);

            // Relationship: Payment -> Order
            entity.HasOne(e => e.Order)
                .WithMany(o => o.Payments)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relationship: Payment -> Creator (User)
            entity.HasOne(e => e.Creator)
                .WithMany(u => u.PaymentsCreated)
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.CreatedBy);
            entity.HasIndex(e => e.PaidAt);
            entity.HasIndex(e => e.Method);
            
            // Composite index for common query patterns:
            // - Payments by OrderId and PaidAt (for order payment history with date sorting)
            // - Payments by Method and PaidAt (for payment method analysis with date filtering)
            entity.HasIndex(e => new { e.OrderId, e.PaidAt });
            entity.HasIndex(e => new { e.Method, e.PaidAt });
        });

        // GiftCard configuration
        modelBuilder.Entity<GiftCard>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Balance).HasColumnType("decimal(18,2)");
            entity.Property(e => e.OriginalAmount).HasColumnType("decimal(18,2)");

            // Unique constraint on Code
            entity.HasIndex(e => e.Code).IsUnique();

            // Relationship: GiftCard -> Business
            entity.HasOne(e => e.Business)
                .WithMany()
                .HasForeignKey(e => e.BusinessId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.BusinessId);
        });
    }
}
