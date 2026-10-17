
using AutoMapper;
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
        private readonly IMapper mapper;
        private readonly IEmailService emailService;
        private readonly IWebHostEnvironment environment;

        public SanctionLetterService(
            AppDbContext db,
            IMapper mapper,
            IEmailService emailService,
            IWebHostEnvironment environment)
        {
            this.db = db;
            this.mapper = mapper;
            this.emailService = emailService;
            this.environment = environment;
        }

        // Existing non-paged method
        public async Task<List<LoanApplyDto>> GetApprovedLoansAsync()
        {
            var loans = await db.LoanDeals
                .AsNoTracking()
                .Include(x => x.Customer)
                .Where(x =>
                    x.CurrentStatus == "Approved" &&
                    x.ApprovedAmount > 0)
                .OrderByDescending(x => x.AppliedDate)
                .ToListAsync();

            var items = mapper.Map<List<LoanApplyDto>>(loans);

            for (int i = 0; i < loans.Count; i++)
            {
                var loan = loans[i];

                items[i].CustomerId = loan.CustomerId;
                items[i].CustomerName = loan.Customer == null
                    ? string.Empty
                    : $"{loan.Customer.FirstName} {loan.Customer.LastName}".Trim();

                items[i].MonthlyIncome = loan.Customer?.MonthlyIncome ?? 0;
                items[i].BankName = loan.BankName;
            }

            return items;
        }

        // Get approved loans for the officer's paged screen
        public async Task<PagedResult<ApprovedLoanDto>> GetApprovedLoansAsync(
            int page,
            int pageSize)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = db.LoanDeals
                .AsNoTracking()
                .Include(x => x.Customer)
                .Where(x =>
                    x.CurrentStatus == "Approved" &&
                    x.ApprovedAmount > 0);

            var totalCount = await query.CountAsync();

            var loans = await query
                .OrderByDescending(x => x.AppliedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = mapper.Map<List<ApprovedLoanDto>>(loans);

            for (int i = 0; i < loans.Count; i++)
            {
                var loan = loans[i];

                items[i].CustomerId = loan.CustomerId;
                items[i].CustomerName = loan.Customer == null
                    ? string.Empty
                    : $"{loan.Customer.FirstName} {loan.Customer.LastName}".Trim();

                items[i].MonthlyIncome = loan.Customer?.MonthlyIncome ?? 0;
                items[i].BankName = loan.BankName;
            }

            return PagedResult<ApprovedLoanDto>.Create(
                items, page, pageSize, totalCount);
        }

        // Generate, save and email sanction letter
        public async Task<SanctionLetterDocumentDto>
            GenerateSanctionLetterAsync(int dealId)
        {
            var loan = await db.LoanDeals
                .Include(x => x.Customer)
                .FirstOrDefaultAsync(x => x.DealId == dealId);

            if (loan == null)
                throw new KeyNotFoundException("Loan deal not found.");

            if (loan.CurrentStatus != "Approved" ||
                loan.ApprovedAmount <= 0)
            {
                throw new InvalidOperationException(
                    "Only approved loans with an approved amount can receive a sanction letter.");
            }

            if (loan.Customer == null)
            {
                throw new InvalidOperationException(
                    "Customer details could not be found.");
            }

            if (string.IsNullOrWhiteSpace(loan.Customer.Email))
            {
                throw new InvalidOperationException(
                    "Customer email address is missing.");
            }

            var documentsFolder = Path.Combine(
                environment.WebRootPath ?? environment.ContentRootPath,
                "Documents");

            Directory.CreateDirectory(documentsFolder);

            var fileName = $"SanctionLetter_{loan.DealId}.pdf";
            var filePath = Path.Combine(documentsFolder, fileName);

            QuestPDF.Settings.License = LicenseType.Community;

            var customerName =
                $"{loan.Customer.FirstName} {loan.Customer.LastName}".Trim();

            Document.Create(container =>
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
                                .FontSize(24)
                                .Bold()
                                .FontColor(Colors.Blue.Darken2);

                            column.Item()
                                .AlignCenter()
                                .Text("LOAN SANCTION LETTER")
                                .FontSize(16)
                                .Bold();

                            column.Item()
                                .PaddingTop(10)
                                .LineHorizontal(1)
                                .LineColor(Colors.Grey.Medium);
                        });

                    page.Content()
                        .PaddingVertical(20)
                        .Column(column =>
                        {
                            column.Spacing(10);

                            column.Item()
                                .AlignRight()
                                .Text($"Date: {DateTime.Now:dd-MM-yyyy}");

                            column.Item()
                                .Text($"Dear {customerName},");

                            column.Item().Text(
                                "We are pleased to inform you that your loan application has been approved. Please find the sanctioned loan details below.");

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
                                            .BorderColor(Colors.Grey.Lighten1)
                                            .Padding(8)
                                            .Text(label)
                                            .Bold();

                                        table.Cell()
                                            .Border(1)
                                            .BorderColor(Colors.Grey.Lighten1)
                                            .Padding(8)
                                            .Text(value);
                                    }

                                    AddRow("Deal ID", loan.DealId.ToString());
                                    AddRow("Customer ID", loan.CustomerId.ToString());
                                    AddRow("Customer Name", customerName);
                                    AddRow("Monthly Income",
                                        loan.Customer.MonthlyIncome.ToString("N2"));
                                    AddRow("Bank Name", loan.BankName);
                                    AddRow("Loan Type", loan.LoanType);
                                    AddRow("Requested Amount",
                                        loan.LoanAmount.ToString("N2"));
                                    AddRow("Sanctioned Amount",
                                        loan.ApprovedAmount.ToString("N2"));
                                    AddRow("Interest Rate",
                                        $"{loan.InterestRate}%");
                                    AddRow("Tenure",
                                        $"{loan.TenureMonths} months");
                                    AddRow("Monthly EMI",
                                        loan.EmiAmount.ToString("N2"));
                                });

                            column.Item()
                                .PaddingTop(15)
                                .Text(
                                    "Please retain this letter for your records. Loan disbursement will be subject to the applicable loan agreement and bank requirements.");

                            column.Item().PaddingTop(15).Text("Regards,");

                            column.Item()
                                .Text("FINCORE Loan Department")
                                .Bold();
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text("This is a system-generated sanction letter.")
                        .FontSize(9)
                        .FontColor(Colors.Grey.Darken1);
                });
            }).GeneratePdf(filePath);

            var existingLetter = await db.SanctionLetters
                .FirstOrDefaultAsync(x => x.DealId == loan.DealId);

            if (existingLetter == null)
            {
                existingLetter = new SanctionLetter
                {
                    DealId = loan.DealId
                };

                db.SanctionLetters.Add(existingLetter);
            }

            await db.SaveChangesAsync();

            await emailService.SendEmailWithAttachmentAsync(
                loan.Customer.Email,
                $"FINCORE Loan Sanction Letter - Deal #{loan.DealId}",
                $"Dear {customerName},{Environment.NewLine}{Environment.NewLine}" +
                "Your loan has been approved. Please find your sanction letter attached as a PDF document." +
                $"{Environment.NewLine}{Environment.NewLine}" +
                "Regards," + Environment.NewLine +
                "FINCORE Loan Department",
                filePath,
                fileName);

            var response =
                mapper.Map<SanctionLetterDocumentDto>(existingLetter);

            response.FileName = fileName;
            response.Message =
                "Sanction letter generated and emailed successfully.";

            return response;
        }

        public async Task<SanctionLetterDocumentDto?>
            GetSanctionLetterAsync(int dealId)
        {
            var letter = await db.SanctionLetters
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.DealId == dealId);

            if (letter == null)
                return null;

            var response =
                mapper.Map<SanctionLetterDocumentDto>(letter);

            response.FileName = $"SanctionLetter_{dealId}.pdf";

            return response;
        }
    }
}
