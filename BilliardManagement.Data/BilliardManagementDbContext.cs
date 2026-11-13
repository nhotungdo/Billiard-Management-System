using System.Reflection;
using Microsoft.EntityFrameworkCore;
using BilliardManagement.Common.Exceptions;
using BilliardManagement.Common.Tenant;
using BilliardManagement.Models.Models;
using BilliardManagement.Models.Tenant;

namespace BilliardManagement.Data
{
    public class BilliardManagementDbContext : DbContext
    {
        public static readonly Guid DefaultStoreId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        private readonly ITenantContext _tenantContext;

        public BilliardManagementDbContext(
            DbContextOptions<BilliardManagementDbContext> options,
            ITenantContext? tenantContext = null)
            : base(options)
        {
            _tenantContext = tenantContext ?? new TenantContext();
        }

        public ITenantContext Tenant => _tenantContext;

        public DbSet<Store> Stores { get; set; }
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
        public DbSet<SessionCombo> SessionCombos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Store configurations
            modelBuilder.Entity<Store>()
                .HasIndex(s => s.Name);

            modelBuilder.Entity<Store>()
                .HasMany(s => s.Users)
                .WithOne(u => u.Store)
                .HasForeignKey(u => u.StoreId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Store>()
                .HasMany(s => s.BilliardTables)
                .WithOne()
                .HasForeignKey(t => t.StoreId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Store>()
                .HasMany(s => s.Customers)
                .WithOne()
                .HasForeignKey(c => c.StoreId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Store>()
                .HasMany(s => s.Products)
                .WithOne()
                .HasForeignKey(p => p.StoreId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Store>()
                .HasMany(s => s.Categories)
                .WithOne()
                .HasForeignKey(c => c.StoreId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Store>()
                .HasMany(s => s.Combos)
                .WithOne()
                .HasForeignKey(c => c.StoreId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Store>()
                .HasMany(s => s.ComboItems)
                .WithOne()
                .HasForeignKey(ci => ci.StoreId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Store>()
                .HasMany(s => s.TableSessions)
                .WithOne()
                .HasForeignKey(ts => ts.StoreId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Store>()
                .HasMany(s => s.TableStatusHistories)
                .WithOne()
                .HasForeignKey(h => h.StoreId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Store>()
                .HasMany(s => s.Shifts)
                .WithOne()
                .HasForeignKey(sh => sh.StoreId)
                .OnDelete(DeleteBehavior.Restrict);

            // Customer configurations
            modelBuilder.Entity<Customer>()
                .HasIndex(c => new { c.StoreId, c.PhoneNumber })
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
                .HasIndex(t => new { t.StoreId, t.TableName })
                .IsUnique();

            modelBuilder.Entity<BilliardTable>()
                .Property(t => t.HourlyRate)
                .HasPrecision(18, 2);

            modelBuilder.Entity<TableSession>()
                .Property(s => s.TotalPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<TableSession>()
                .Property(s => s.ComboPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Product>()
                .HasIndex(p => new { p.StoreId, p.ProductName })
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
                .HasIndex(c => new { c.StoreId, c.ComboCode })
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

            modelBuilder.Entity<SessionCombo>()
                .Property(sc => sc.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<SessionCombo>()
                .HasOne(sc => sc.TableSession)
                .WithMany(ts => ts.SessionCombos)
                .HasForeignKey(sc => sc.TableSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Tenant indexes
            modelBuilder.Entity<BilliardTable>().HasIndex(e => e.StoreId);
            modelBuilder.Entity<TableSession>().HasIndex(e => e.StoreId);
            modelBuilder.Entity<TableStatusHistory>().HasIndex(e => e.StoreId);
            modelBuilder.Entity<Customer>().HasIndex(e => e.StoreId);
            modelBuilder.Entity<Product>().HasIndex(e => e.StoreId);
            modelBuilder.Entity<Category>().HasIndex(e => e.StoreId);
            modelBuilder.Entity<Combo>().HasIndex(e => e.StoreId);
            modelBuilder.Entity<ComboItem>().HasIndex(e => e.StoreId);
            modelBuilder.Entity<Shift>().HasIndex(e => e.StoreId);
            modelBuilder.Entity<User>().HasIndex(e => e.StoreId);

            // Global query filters for tenant entities
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (!typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType)) continue;

                var method = typeof(BilliardManagementDbContext)
                    .GetMethod(nameof(ApplyTenantFilter), BindingFlags.Instance | BindingFlags.NonPublic)!
                    .MakeGenericMethod(entityType.ClrType);
                method.Invoke(this, new object[] { modelBuilder });
            }

            // Users can belong to no store (SuperAdmin), so they need a custom filter
            modelBuilder.Entity<User>().HasQueryFilter(u =>
                !Tenant.IsResolved || (Tenant.IsSuperAdmin && Tenant.StoreId == Guid.Empty) ||
                u.Role == BilliardManagement.Models.Enums.UserRole.SuperAdmin ||
                (u.StoreId.HasValue && u.StoreId.Value == Tenant.StoreId));

            // Derived tenant data inherits isolation through its aggregate root
            modelBuilder.Entity<Order>().HasQueryFilter(o =>
                !Tenant.IsResolved || (Tenant.IsSuperAdmin && Tenant.StoreId == Guid.Empty) ||
                (o.TableSession != null && o.TableSession.StoreId == Tenant.StoreId));

            modelBuilder.Entity<OrderItem>().HasQueryFilter(oi =>
                !Tenant.IsResolved || (Tenant.IsSuperAdmin && Tenant.StoreId == Guid.Empty) ||
                (oi.Order != null && oi.Order.TableSession != null && oi.Order.TableSession.StoreId == Tenant.StoreId));

            modelBuilder.Entity<Invoice>().HasQueryFilter(i =>
                !Tenant.IsResolved || (Tenant.IsSuperAdmin && Tenant.StoreId == Guid.Empty) ||
                (i.TableSession != null && i.TableSession.StoreId == Tenant.StoreId));

            modelBuilder.Entity<SessionCombo>().HasQueryFilter(sc =>
                !Tenant.IsResolved || (Tenant.IsSuperAdmin && Tenant.StoreId == Guid.Empty) ||
                (sc.TableSession != null && sc.TableSession.StoreId == Tenant.StoreId));
        }

        private void ApplyTenantFilter<TEntity>(ModelBuilder modelBuilder) where TEntity : class, ITenantEntity
        {
            modelBuilder.Entity<TEntity>().HasQueryFilter(e =>
                !Tenant.IsResolved || (Tenant.IsSuperAdmin && Tenant.StoreId == Guid.Empty) || e.StoreId == Tenant.StoreId);
        }

        public override int SaveChanges()
        {
            ApplyTenantRules();
            return base.SaveChanges();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            ApplyTenantRules();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyTenantRules();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            ApplyTenantRules();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void ApplyTenantRules()
        {
            foreach (var entry in ChangeTracker.Entries<ITenantEntity>())
            {
                if (Tenant.IsResolved && (!Tenant.IsSuperAdmin || Tenant.StoreId != Guid.Empty) &&
                    (entry.State == EntityState.Modified || entry.State == EntityState.Deleted) &&
                    entry.Entity.StoreId != Tenant.StoreId)
                    throw new CustomException("Không được phép sửa hoặc xóa dữ liệu cửa hàng khác.", 403);

                if (entry.State == EntityState.Added)
                {
                    if (entry.Entity.StoreId != Guid.Empty)
                    {
                        if (Tenant.IsResolved && (!Tenant.IsSuperAdmin || Tenant.StoreId != Guid.Empty) &&
                            entry.Entity.StoreId != Tenant.StoreId)
                            throw new CustomException("Không được phép tạo dữ liệu cho cửa hàng khác.", 403);

                        continue;
                    }

                    if (!Tenant.IsResolved)
                    {
                        // Seeding / design-time: all legacy data belongs to the default store.
                        entry.Entity.StoreId = DefaultStoreId;
                    }
                    else if (!Tenant.IsSuperAdmin)
                    {
                        if (Tenant.StoreId == Guid.Empty)
                            throw new CustomException("Không xác định được cửa hàng hiện tại.");

                        entry.Entity.StoreId = Tenant.StoreId;
                    }
                    else
                    {
                        if (Tenant.StoreId == Guid.Empty)
                            throw new CustomException(
                                "SuperAdmin phải chỉ định cửa hàng (header X-Store-Id) khi tạo dữ liệu nghiệp vụ.");

                        entry.Entity.StoreId = Tenant.StoreId;
                    }
                }
                else if (entry.State == EntityState.Modified)
                {
                    var original = entry.Property(nameof(ITenantEntity.StoreId)).OriginalValue;
                    if (original is Guid originalStoreId &&
                        originalStoreId != Guid.Empty &&
                        originalStoreId != entry.Entity.StoreId)
                    {
                        throw new CustomException(
                            "Không được phép thay đổi StoreId của dữ liệu nghiệp vụ.");
                    }
                }
            }
        }
    }
}
