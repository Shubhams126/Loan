
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
        IBackgroundJobClient client;
        IEmiScheduleService service;

        public EmiScheduleController(
        IBackgroundJobClient client,
        IEmiScheduleService service)
        {
            this.client = client;
            this.service = service;
        }

        [HttpPost("generate/{loanAccountId}")]
        public IActionResult GenerateEmiSchedule(int loanAccountId)
        {
            var jobId = client.Enqueue<IEmiScheduleService>(
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
            var schedule = await service.GetEmiSchedule(loanAccountId);

            return Ok(schedule);
        }
    }
}
