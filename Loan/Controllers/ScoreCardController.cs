using Loan.Application.Interface;
using Loan.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Loan.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScoreCardController : ControllerBase
    {
        IScoreCardService service;
        public ScoreCardController(IScoreCardService service)
        {
            this.service = service;
        }

        [Authorize(Roles = "Officer")]
        [HttpPost]
        [Route("generate/{customerId}")]
        public async Task<IActionResult> GenerateScoreCard(int customerId)
        {
            var scoreCard = await service.GenerateScoreCard(customerId);

            if (scoreCard == null)
            {
                return NotFound("Customer not found or CIBIL score not calculated yet");
            }

            var result = new
            {
                scoreCard.ScoreCardId,
                scoreCard.CustomerId,
                scoreCard.RiskCategory,
                scoreCard.EligibleLoanAmount
            };

            var response = new ApiResponse<object>
            {
                Success = true,
                Message = "ScoreCard generated successfully",
                Data = result,
                Error = null
            };

            return Ok(response);
        }

        [Authorize(Roles = "Officer")]
        [HttpGet]
        [Route("get/{customerId}")]
        public async Task<IActionResult> GetScoreCard(int customerId)
        {
            var scoreCard = await service.GetScoreCard(customerId);

            if (scoreCard == null)
            {
                return NotFound("No ScoreCard found for this customer");
            }

            var result = new
            {
                scoreCard.ScoreCardId,
                scoreCard.CustomerId,
                scoreCard.RiskCategory,
                scoreCard.EligibleLoanAmount
            };

            var response = new ApiResponse<object>
            {
                Success = true,
                Message = "ScoreCard retrieved successfully",
                Data = result,
                Error = null
            };

            return Ok(response);
        }
    }
}