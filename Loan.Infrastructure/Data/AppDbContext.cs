using Loan.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Loan.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; }

        public DbSet<KycDocument> KycDocuments { get; set; }

        public DbSet<CibilReport> CibilReports { get; set; }

        public DbSet<EligibilityResult> EligibilityResults { get; set; }

        public DbSet<ScoreCard> ScoreCards { get; set; }

        public DbSet<LoanDeal> LoanDeals { get; set; }

        public DbSet<DealReview> DealReviews { get; set; }

        public DbSet<SanctionLetter> SanctionLetters { get; set; }

        public DbSet<Disbursement> Disbursements { get; set; }

        public DbSet<Role> Roles { get; set; }

        public DbSet<LoanAccount> LoanAccounts { get; set; }

        public DbSet<EmiSchedule> EmiSchedules { get; set; }

        public DbSet<LoanPayment> LoanPayments { get; set; }

        public DbSet<InterestAccrual> InterestAccruals { get; set; }

        public DbSet<PenaltyCharge> PenaltyCharges { get; set; }

        public DbSet<ForeClosureRequest> ForeClosureRequests { get; set; }

        public DbSet<LoanClosure> LoanClosures { get; set; }

        public DbSet<Notification> Notifications { get; set; }
        public DbSet<SupportTicket> SupportTickets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var relationship in modelBuilder.Model
                .GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior =
                    DeleteBehavior.NoAction;
            }
        }
    }
}
