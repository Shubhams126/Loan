using Loan.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Loan.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<LoanAccount> LoanAccounts { get; set; }

        public DbSet<Disbursement> Disbursements { get; set; }

        public DbSet<LoanDeal> LoanDeals { get; set; }

        public DbSet<EmiSchedule> EmiSchedules { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Customer>()
                .ToTable("Customers");
        }
    }
}