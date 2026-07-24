using Microsoft.EntityFrameworkCore;
using BilliardManagement.Models.Models;

namespace BilliardManagement.Data
{
    public class BilliardManagementDbContext : DbContext
    {
        public BilliardManagementDbContext(DbContextOptions<BilliardManagementDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<BilliardTable> BilliardTables { get; set; }
        public DbSet<TableSession> TableSessions { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<TableStatusHistory> TableStatusHistories { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Combo> Combos { get; set; }
        public DbSet<ComboItem> ComboItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Customer configurations
            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.PhoneNumber)
                .IsUnique();

            modelBuilder.Entity<Customer>()
                .Property(c => c.TotalSpent)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Customer>()
                .Property(c => c.TotalPlayHours)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Customer>()
                .HasMany(c => c.TableSessions)
                .WithOne(ts => ts.Customer)
                .HasForeignKey(ts => ts.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Customer>()
                .HasMany(c => c.Invoices)
                .WithOne(i => i.Customer)
                .HasForeignKey(i => i.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            // Precision for decimal properties
            modelBuilder.Entity<BilliardTable>()
                .HasIndex(t => t.TableName)
                .IsUnique();

            modelBuilder.Entity<BilliardTable>()
                .Property(t => t.HourlyRate)
                .HasPrecision(18, 2);

            modelBuilder.Entity<TableSession>()
                .Property(s => s.TotalPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.ProductName)
                .IsUnique();

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.TotalPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Invoice>()
                .Property(i => i.Subtotal)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Invoice>()
                .Property(i => i.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Shift>()
                .Property(s => s.TotalRevenue)
                .HasPrecision(18, 2);

            // Fix multiple cascade paths
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.OrderedBy)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.TableSession)
                .WithMany(s => s.Invoices)
                .HasForeignKey(i => i.TableSessionId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<TableStatusHistory>()
                .HasOne(h => h.BilliardTable)
                .WithMany()
                .HasForeignKey(h => h.TableId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TableStatusHistory>()
                .HasOne(h => h.ChangedByUser)
                .WithMany()
                .HasForeignKey(h => h.ChangedById)
                .OnDelete(DeleteBehavior.SetNull);

            // Combo configurations
            modelBuilder.Entity<Combo>()
                .HasIndex(c => c.ComboCode)
                .IsUnique();

            modelBuilder.Entity<Combo>()
                .Property(c => c.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Combo>()
                .HasMany(c => c.ComboItems)
                .WithOne(ci => ci.Combo)
                .HasForeignKey(ci => ci.ComboId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ComboItem>()
                .HasOne(ci => ci.Product)
                .WithMany()
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
