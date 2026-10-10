using Loan.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Loan.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Authentication and registration
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        //public DbSet<PendingRegistration> PendingRegistrations { get; set; }

        // Customer onboarding and eligibility
        public DbSet<Customer> Customers { get; set; }
        public DbSet<KycDocument> KycDocuments { get; set; }
        public DbSet<CibilReport> CibilReports { get; set; }
        public DbSet<EligibilityResult> EligibilityResults { get; set; }
        public DbSet<ScoreCard> ScoreCards { get; set; }

        // Loan application and approval
        public DbSet<LoanDeal> LoanDeals { get; set; }
        public DbSet<DealReview> DealReviews { get; set; }
        public DbSet<SanctionLetter> SanctionLetters { get; set; }
        public DbSet<Disbursement> Disbursements { get; set; }

        // Loan account and repayments
        public DbSet<LoanAccount> LoanAccounts { get; set; }
        public DbSet<EmiSchedule> EmiSchedules { get; set; }
        public DbSet<LoanPayment> LoanPayments { get; set; }
        public DbSet<InterestAccrual> InterestAccruals { get; set; }
        public DbSet<PenaltyCharge> PenaltyCharges { get; set; }

        // Loan closure and support
        public DbSet<ForeClosureRequest> ForeClosureRequests { get; set; }
        public DbSet<LoanClosure> LoanClosures { get; set; }
        public DbSet<SupportTicket> SupportTickets { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, RoleName = "Customer" },
                new Role { RoleId = 2, RoleName = "LoanOfficer" }
            );

            // Unique indexes
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.Email)
                .IsUnique();

            modelBuilder.Entity<LoanAccount>()
                .HasIndex(l => l.LoanAccountNo)
                .IsUnique();

            // User - Role: many users can have one role
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // User - Customer: optional one-to-one relationship
            modelBuilder.Entity<User>()
                .HasOne(u => u.Customer)
                .WithOne()
                .HasForeignKey<User>(u => u.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Customer-related records
            modelBuilder.Entity<KycDocument>()
                .HasOne(k => k.Customer)
                .WithMany(c => c.KycDocuments)
                .HasForeignKey(k => k.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CibilReport>()
                .HasOne(c => c.Customer)
                .WithMany(cu => cu.CibilReports)
                .HasForeignKey(c => c.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EligibilityResult>()
                .HasOne(e => e.Customer)
                .WithMany(c => c.EligibilityResults)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ScoreCard>()
                .HasOne(s => s.Customer)
                .WithMany(c => c.ScoreCards)
                .HasForeignKey(s => s.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Customer - LoanDeal
            modelBuilder.Entity<LoanDeal>()
                .HasOne(l => l.Customer)
                .WithMany(c => c.LoanDeals)
                .HasForeignKey(l => l.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // LoanDeal - DealReview
            modelBuilder.Entity<DealReview>()
                .HasOne(d => d.LoanDeal)
                .WithMany(l => l.DealReviews)
                .HasForeignKey(d => d.DealId)
                .OnDelete(DeleteBehavior.Restrict);

            // User (Loan Officer) - DealReview
            modelBuilder.Entity<DealReview>()
                .HasOne(d => d.User)
                .WithMany(u => u.DealReviews)
                .HasForeignKey(d => d.OfficerId)
                .OnDelete(DeleteBehavior.Restrict);

            // LoanDeal - SanctionLetter
            modelBuilder.Entity<SanctionLetter>()
                .HasOne(s => s.LoanDeal)
                .WithMany(l => l.SanctionLetters)
                .HasForeignKey(s => s.DealId)
                .OnDelete(DeleteBehavior.Restrict);

            // LoanDeal - Disbursement
            modelBuilder.Entity<Disbursement>()
                .HasOne(d => d.LoanDeal)
                .WithMany(l => l.Disbursements)
                .HasForeignKey(d => d.DealId)
                .OnDelete(DeleteBehavior.Restrict);

            // LoanAccount - Customer and LoanDeal
            modelBuilder.Entity<LoanAccount>()
                .HasOne(l => l.Customer)
                .WithMany()
                .HasForeignKey(l => l.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LoanAccount>()
                .HasOne(l => l.LoanDeal)
                .WithMany()
                .HasForeignKey(l => l.DealId)
                .OnDelete(DeleteBehavior.Restrict);

            // LoanAccount-related records
            modelBuilder.Entity<EmiSchedule>()
                .HasOne(e => e.LoanAccount)
                .WithMany()
                .HasForeignKey(e => e.LoanAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LoanPayment>()
                .HasOne(p => p.LoanAccount)
                .WithMany()
                .HasForeignKey(p => p.LoanAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InterestAccrual>()
                .HasOne(i => i.LoanAccount)
                .WithMany()
                .HasForeignKey(i => i.LoanAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PenaltyCharge>()
                .HasOne(p => p.LoanAccount)
                .WithMany()
                .HasForeignKey(p => p.LoanAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PenaltyCharge>()
                .HasOne(p => p.EmiSchedule)
                .WithMany()
                .HasForeignKey(p => p.EmiScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Foreclosure request
            modelBuilder.Entity<ForeClosureRequest>()
                .HasOne(f => f.LoanAccount)
                .WithMany()
                .HasForeignKey(f => f.LoanAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ForeClosureRequest>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(f => f.ClosedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // Loan closure
            modelBuilder.Entity<LoanClosure>()
                .HasOne(l => l.LoanAccount)
                .WithMany()
                .HasForeignKey(l => l.LoanAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LoanClosure>()
                .HasOne(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.ClosedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // Notification has CustomerId but no navigation property
            modelBuilder.Entity<Notification>()
                .HasOne<Customer>()
                .WithMany()
                .HasForeignKey(n => n.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Support tickets
            modelBuilder.Entity<SupportTicket>()
                .HasOne(s => s.Customer)
                .WithMany()
                .HasForeignKey(s => s.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SupportTicket>()
                .HasOne(s => s.LoanAccount)
                .WithMany()
                .HasForeignKey(s => s.LoanAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
