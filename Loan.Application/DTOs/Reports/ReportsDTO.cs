using System;
using System.Collections.Generic;

namespace Loan.Application.DTOs.Reports
{
    // Main DTO returned by the LOS Reports Dashboard API.
    public class ReportsDashboardDTO
    {
        public ReportsSummaryDTO Summary { get; set; } = new();

        public List<ApplicationFunnelDTO> ApplicationFunnel { get; set; } = new();

        public List<CibilScoreRangeDTO> CibilScoreRanges { get; set; } = new();

        public List<ScoreCardWiseDTO> ScoreCardWise { get; set; } = new();

        public List<ProductWiseDTO> ProductWise { get; set; } = new();
    }

    // Contains the overall application summary.
    public class ReportsSummaryDTO
    {
        public int TotalApplications { get; set; }

        public int ApplicationsApproved { get; set; }

        public int ApplicationsRejected { get; set; }

        public int ApplicationsPending { get; set; }
    }

    // Shows how many applications reached each stage.
    public class ApplicationFunnelDTO
    {
        public string Stage { get; set; } = string.Empty;

        public int Count { get; set; }
    }

    // Groups applicants by their CIBIL score range.
    public class CibilScoreRangeDTO
    {
        public string CibilScoreRange { get; set; } = string.Empty;

        public int Applicants { get; set; }

        public int Approved { get; set; }

        public decimal ApprovalPercentage { get; set; }
    }

    // Shows application results for each scorecard.
    public class ScoreCardWiseDTO
    {
        public string ScoreCard { get; set; } = string.Empty;

        public int Applicants { get; set; }

        public int Approved { get; set; }

        public int Rejected { get; set; }

        public int Pending { get; set; }

        public decimal ApprovalPercentage { get; set; }
    }

    // Shows application and disbursement statistics by loan product.
    public class ProductWiseDTO
    {
        public string Product { get; set; } = string.Empty;

        public int TotalApplications { get; set; }

        public int Approved { get; set; }

        public int Disbursed { get; set; }

        public decimal DisbursementPercentage { get; set; }
    }

    // Represents the average and range of processing time per stage.
    public class TurnaroundTimeDTO
    {
        public string Stage { get; set; } = string.Empty;

        public decimal AverageDays { get; set; }

        public int MinimumDays { get; set; }

        public int MaximumDays { get; set; }
    }
}

