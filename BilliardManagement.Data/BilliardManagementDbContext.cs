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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
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
                .Property(i => i.Discount)
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
        }
    }
}
