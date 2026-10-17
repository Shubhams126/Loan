namespace Loan.Application.DTOs.Reports
{
    // Main DTO for the LMS Reports Dashboard.
    public class LMSReportsDashboardDTO
    {
        // Summary cards displayed at the top of the dashboard.
        public LMSReportsSummaryDTO Summary { get; set; } = new();

        // Shows outstanding loan amounts grouped by loan product.
        public List<LoanPortfolioChartDTO> LoanPortfolioChart { get; set; } = new();

        // Shows the percentage of EMI collections for each month.
        public List<CollectionEfficiencyChartDTO> CollectionEfficiencyChart { get; set; } = new();
    }

    // Contains the summary values shown on dashboard cards.
    public class LMSReportsSummaryDTO
    {
        public int TotalLoans { get; set; }

        public int ActiveLoans { get; set; }

        public int ClosedLoans { get; set; }
    }

    // Groups outstanding loan balances by loan product.
    public class LoanPortfolioChartDTO
    {
        public string Product { get; set; } = string.Empty;

        public decimal OutstandingAmount { get; set; }

        public decimal Percentage { get; set; }
    }

    // Shows monthly EMI collection efficiency.
    public class CollectionEfficiencyChartDTO
    {
        public string Month { get; set; } = string.Empty;

        public decimal CollectionPercentage { get; set; }
    }
}

