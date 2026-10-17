using Loan.Application.ApisResponses;
using Loan.Application.DTOs;
using Loan.Application.Interface;
using Loan.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Ocsp;

namespace Loan.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ForeClosureController : ControllerBase
    {
        IForeClosureService service;
        IForeClosureService ser;
        public ForeClosureController(IForeClosureService service,
            IForeClosureService ser)
        {
            this.service= service;
            this.ser= ser;
        }

        [HttpPost]
        [Route("CreateForeClousre")]
        public async Task<APIResponse<IActionResult>> CreateForeClosureRequest(CreateForeclosureDto dto)
        {
           var res= await service.CreateForeClosureRequest(dto);
            if (res != "Success")
            {
                return APIResponse<IActionResult>.ErrorResponse(res);
            }
            return APIResponse<IActionResult>.SuccessResponse(null,"ForeClosure Request Added Successfully");
        }

        
        [HttpGet]
        [Route("GetForeClousres")]
        public async Task<APIResponse<List<ForeClosureRequestDto>>> GetRequests()
        {
            var req=await service.GetForeClosureRequests();
            return APIResponse<List<ForeClosureRequestDto>>.SuccessResponse(req);
        }

        [HttpPut]
        [Route("UpdateForeClousre/{id}")]
        public async Task<APIResponse<IActionResult>> UpdateRequest(int id, UpdateForeClosureDto dto)
        {
            await service.UpdateForeClosuere(id, dto);
            return APIResponse<IActionResult>.SuccessResponse(null,"Updated Successfully");

        }


        [HttpGet]
        [Route("GetN")]
        public async Task<IActionResult> Name(string n)
        {
            var na = n;
            var rev = " ";
            for(int i= na.Length-1;i>=0;i--)
            {
                rev += na[i];
            }
            return Ok(new { data = rev });
        }

        [HttpPost]
        [Route("MakeForeClousrePayment")]
        public async Task<APIResponse<IActionResult>> MakeForeClousrePayment(int id)
        {
            var rq = await service.PayForeClosure(id);
            return APIResponse<IActionResult>.SuccessResponse(null,"Payment Done Successfully");
        }

        [HttpGet]
        [Route("FCGetBy")]
        public async Task<APIResponse<List<ForeClosureRequest>>> GetForeClousreByid()
        {
            var customerClaim = User.FindFirst("CustomerId");
            if (customerClaim == null)
            {
                return APIResponse<List<ForeClosureRequest>>
                    .ErrorResponse("CustomerId claim not found. Please check login authentication.");
            }

            if (!int.TryParse(customerClaim.Value, out int id))
            {
                return APIResponse<List<ForeClosureRequest>>
                    .ErrorResponse("Invalid CustomerId claim.");
            }
            var reqs = await service.GetForeClosureRequestbyid(id);
            return APIResponse<List<ForeClosureRequest>>.SuccessResponse(reqs);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<APIResponse<ForeClosureRequestDto>> GetForeClosure(int id)
        {
            var reqs = await service.GetForeClosure(id);

            return APIResponse<ForeClosureRequestDto>.SuccessResponse(reqs);
        }

        [HttpPost]
        [Route("CreateForeClosureOrder/{id}")]
        public async Task<IActionResult> CreateForeClosureOrder(int id)
        {
            var result = await ser.CreateForeClosureOrder(id);

            if (result.StartsWith("order_"))
            {
                return Ok(new
                {
                    orderId = result
                });
            }

            return BadRequest(new
            {
                message = result
            });
        }

        [HttpPost]
        [Route("VerifyForeClosurePayment")]
        public async Task<IActionResult> VerifyForeClosurePayment(VerifyForeClosurePaymentDto dto)
        {
            bool result = await ser.VerifyForeClosurePayment(dto);
            if (!result)
                return BadRequest(new
                {
                    message = "Foreclosure payment verification failed"
                });
            return Ok(new
            {
                message = "Foreclosure payment successful"
            });
        }
    }
}
    