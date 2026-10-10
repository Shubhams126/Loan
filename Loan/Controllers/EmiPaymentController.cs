using Loan.Application.DTOs;
using Loan.Application.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Loan.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmiPaymentController : ControllerBase
    {
        IEmiPaymentService service;
        public EmiPaymentController(IEmiPaymentService service)
        {
            this.service = service;
        }
        [HttpPost]
        [Route("MakeEmiPayment")]
        public async Task<IActionResult> MakeEmiPayment(CreateEmiPaymentDto dto)
        {
            var emipayment = await service.MakeEmiPayment(dto);
            return Ok(emipayment);
        }
    }
}
