using Loan.Application.ApisResponses;
using Loan.Application.DTOs;
using Loan.Application.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Loan.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentHistroy : ControllerBase
    {
        IPaymentHistroyService service;
        public PaymentHistroy(IPaymentHistroyService service)
        {
            this.service = service;
        }

        [HttpGet]
        [Route("PaymentHistory")]
        public async Task<APIResponse<List<PaymentHistroyDto>>> GetPaymentHistroy(int? id)
        {
            var data= await service.FetchPaymentHistory(id);
            return APIResponse<List<PaymentHistroyDto>>.SuccessResponse(data, "Request Success");
        }
    }
}
