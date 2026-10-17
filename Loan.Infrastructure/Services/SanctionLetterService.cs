using Loan.Application.DTOs;
using Loan.Application.Interface;
using Loan.Domain.Models;
using Loan.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Loan.Infrastructure.Services
{
    public class SanctionLetterService : ISanctionLetterService
    {
        private readonly AppDbContext db;
        private readonly IWebHostEnvironment env;

        public SanctionLetterService(
            AppDbContext db,
            IWebHostEnvironment env)
        {
            this.db = db;
            this.env = env;
        }

        public async Task<List<ApprovedLoanDto>> GetApprovedLoansAsync()
        {
            var loans = await db.LoanDeals
                .Where(x => x.CurrentStatus == "Approved"
                    && x.ApprovedAmount > 0)
                .OrderByDescending(x => x.AppliedDate)
                .ToListAsync();

            var result = new List<ApprovedLoanDto>();

            foreach (var loan in loans)
            {
                result.Add(new ApprovedLoanDto
                {
                    DealId = loan.DealId,
                    CustomerId = loan.CustomerId,
                    LoanType = loan.LoanType,
                    LoanAmount = loan.LoanAmount,
                    ApprovedAmount = loan.ApprovedAmount,
                    InterestRate = loan.InterestRate,
                    TenureMonths = loan.TenureMonths,
                    EmiAmount = loan.EmiAmount,
                    AppliedDate = loan.AppliedDate
                });
            }

            return result;
        }

        public async Task<SanctionLetterDocumentDto>
            GenerateSanctionLetterAsync(int dealId)
        {
            var loan = await db.LoanDeals
                .Include(x => x.Customer)
                .FirstOrDefaultAsync(x => x.DealId == dealId);

            if (loan == null)
            {
                throw new KeyNotFoundException(
                    "Loan deal was not found.");
            }

            if (loan.CurrentStatus != "Approved")
            {
                throw new InvalidOperationException(
                    "A sanction letter can only be generated for an approved loan.");
            }

            if (loan.ApprovedAmount <= 0)
            {
                throw new InvalidOperationException(
                    "The approved loan amount must be greater than zero.");
            }

            var existingLetter = await db.SanctionLetters
                .FirstOrDefaultAsync(x => x.DealId == dealId);

            if (existingLetter != null)
            {
                throw new InvalidOperationException(
                    "A sanction letter already exists for this loan deal.");
            }

            QuestPDF.Settings.License = LicenseType.Community;

            var documentsFolder = Path.Combine(
                env.ContentRootPath,
                "wwwroot",
                "Documents");

            Directory.CreateDirectory(documentsFolder);

            var fileName = $"SanctionLetter_{dealId}.pdf";
            var filePath = Path.Combine(documentsFolder, fileName);

            var customerName = loan.Customer == null
                ? $"Customer {loan.CustomerId}"
                : $"{loan.Customer.FirstName} {loan.Customer.LastName}".Trim();

            QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header()
                        .Column(column =>
                        {
                            column.Item()
                                .AlignCenter()
                                .Text("FINCORE")
                                .Bold()
                                .FontSize(24);

                            column.Item()
                                .AlignCenter()
                                .Text("LOAN SANCTION LETTER")
                                .Bold()
                                .FontSize(16);

                            column.Item()
                                .PaddingTop(10)
                                .LineHorizontal(1);
                        });

                    page.Content()
                        .PaddingVertical(20)
                        .Column(column =>
                        {
                            column.Spacing(12);

                            column.Item()
                                .AlignRight()
                                .Text($"Date: {DateTime.Now:dd-MM-yyyy}");

                            column.Item()
                                .Text($"Dear {customerName},");

                            column.Item()
                                .Text(
                                    "We are pleased to inform you that your loan application has been approved. " +
                                    "The sanctioned loan details are provided below.");

                            column.Item()
                                .PaddingTop(10)
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(3);
                                    });

                                    void AddRow(string label, string value)
                                    {
                                        table.Cell()
                                            .Border(1)
                                            .Padding(8)
                                            .Text(label)
                                            .SemiBold();

                                        table.Cell()
                                            .Border(1)
                                            .Padding(8)
                                            .Text(value);
                                    }

                                    AddRow("Loan Deal ID", loan.DealId.ToString());
                                    AddRow("Customer ID", loan.CustomerId.ToString());
                                    AddRow("Loan Type", loan.LoanType);
                                    AddRow(
                                        "Requested Amount",
                                        loan.LoanAmount.ToString("C2"));

                                    AddRow(
                                        "Sanctioned Amount",
                                        loan.ApprovedAmount.ToString("C2"));

                                    AddRow(
                                        "Interest Rate",
                                        $"{loan.InterestRate}% per annum");

                                    AddRow(
                                        "Tenure",
                                        $"{loan.TenureMonths} months");

                                    AddRow(
                                        "Monthly EMI",
                                        loan.EmiAmount.ToString("C2"));

                                    AddRow("Loan Status", loan.CurrentStatus);
                                });

                            column.Item()
                                .PaddingTop(15)
                                .Text(
                                    "The loan is subject to the applicable terms and conditions " +
                                    "of the lending institution.");

                            column.Item()
                                .PaddingTop(20)
                                .Text("Regards,");

                            column.Item()
                                .Text("Loan Department")
                                .Bold();

                            column.Item()
                                .Text("FINCORE");
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(
                            "This document is generated electronically by FINCORE.")
                        .FontSize(9);
                });
            }).GeneratePdf(filePath);

            try
            {
                var sanctionLetter = new SanctionLetter
                {
                    DealId = loan.DealId,
                    LoanAmount = loan.ApprovedAmount,
                    InterestRate = loan.InterestRate,
                    TenureMonths = loan.TenureMonths,
                    EmiAmount = loan.EmiAmount,
                    CreatedAt = DateTime.Now
                };

                db.SanctionLetters.Add(sanctionLetter);

                await db.SaveChangesAsync();

                return new SanctionLetterDocumentDto
                {
                    DealId = loan.DealId,
                    LoanAmount = loan.ApprovedAmount,
                    InterestRate = loan.InterestRate,
                    TenureMonths = loan.TenureMonths,
                    EmiAmount = loan.EmiAmount,
                    CreatedAt = sanctionLetter.CreatedAt,
                    FileName = fileName,
                    Message = "Sanction letter generated successfully."
                };
            }
            catch
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                throw;
            }
        }

        public async Task<SanctionLetterDocumentDto?>
            GetSanctionLetterAsync(int dealId)
        {
            var sanctionLetter = await db.SanctionLetters
                .FirstOrDefaultAsync(x => x.DealId == dealId);

            if (sanctionLetter == null)
            {
                return null;
            }

            return new SanctionLetterDocumentDto
            {
                DealId = sanctionLetter.DealId,
                LoanAmount = sanctionLetter.LoanAmount,
                InterestRate = sanctionLetter.InterestRate,
                TenureMonths = sanctionLetter.TenureMonths,
                EmiAmount = sanctionLetter.EmiAmount,
                CreatedAt = sanctionLetter.CreatedAt,
                FileName = $"SanctionLetter_{dealId}.pdf",
                Message = "Sanction letter retrieved successfully."
            };
        }
    }
}