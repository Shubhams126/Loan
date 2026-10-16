using Loan.Application.ApisResponses;
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
        IConfiguration config;
        public EmiPaymentController(IEmiPaymentService service, IConfiguration config)
        {
            this.service = service;
            this.config = config;
        }
        [HttpPost]
        [Route("MakeEmiPayment")]
        public async Task<APIResponse<IActionResult>> MakeEmiPayment(CreateEmiPaymentDto dto)
        {
            var emipayment = await service.MakeEmiPayment(dto);
            return APIResponse<IActionResult>.SuccessResponse(null, "Emi Paid Successfully");
        }

        [HttpPost]
        [Route("CreateOrder")]
        public IActionResult CreateOrder(CreateOrderDto dto)
        {
            var orderId = service.CreateRazorpayOrder(dto);
            return Ok(orderId);
        }

        [HttpGet]
        [Route("GetEmiD/{emisid}")]
        public async Task<APIResponse<EmiPaymentDetailsDto>> GetEmiPaymentDetails(int emisid)
        {
            var emis = await service.GetEmiPaymentDetails(emisid);
            return APIResponse<EmiPaymentDetailsDto>.SuccessResponse(emis!);
        }

        [HttpPost]
        [Route("VerifyEmiPayment")]
        public async Task<APIResponse<string>> VerifyPayment(VerifyEmiPaymentDto d)
        {
            var isvalid= await service.VerifyEmiPayment(d);
            if(!isvalid)
            {
                return APIResponse<string>.ErrorResponse("Payment verification failed");
            }
            return APIResponse<string>.SuccessResponse("Verified","Payment Verified Successfully");
        }
    }

}
