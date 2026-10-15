
using Hangfire;
using Loan.Application.Interface;
using Loan.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace Loan.API.Controllers
{

    [Route("api/emi-schedules")]
    [ApiController]
    public class EmiScheduleController : ControllerBase
    {
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IEmiScheduleService _emiScheduleService;

        public EmiScheduleController(
        IBackgroundJobClient backgroundJobClient,
        IEmiScheduleService emiScheduleService)
        {
            _backgroundJobClient = backgroundJobClient;
            _emiScheduleService = emiScheduleService;
        }

        [HttpPost("generate/{loanAccountId}")]
        public IActionResult GenerateEmiSchedule(int loanAccountId)
        {
            var jobId = _backgroundJobClient.Enqueue<IEmiScheduleService>(
                service => service.GenerateEmiSchedule(loanAccountId));

            return Ok(new
            {
                Message = "EMI schedule generation job added to Hangfire",
                JobId = jobId
            });
        }


        [HttpGet("{loanAccountId}")]
        public async Task<IActionResult> GetEmiSchedule(int loanAccountId)
        {
            var schedule = await _emiScheduleService.GetEmiSchedule(loanAccountId);

            return Ok(schedule);
        }
    }
}
