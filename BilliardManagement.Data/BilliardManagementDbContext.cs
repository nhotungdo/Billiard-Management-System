using Microsoft.EntityFrameworkCore;

namespace BilliardManagement.Data
{
    public class BilliardManagementDbContext : DbContext
    {
        public BilliardManagementDbContext(DbContextOptions<BilliardManagementDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Configurations will be added here
        }

        // DbSets will be added here
    }
}
