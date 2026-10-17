using Loan.Application.ApisResponses;
using Loan.Application.DTOs;
using Loan.Application.Interface;
using Loan.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Loan.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ForeClosureController : ControllerBase
    {
        IForeClosureService service;
        public ForeClosureController(IForeClosureService service)
        {
            this.service= service;
        }

        [HttpPost]
        [Route("CreateForeClousre")]
        public async Task<APIResponse<IActionResult>> CreateForeClosureRequest(CreateForeclosureDto dto)
        {
           var res= await service.CreateForeClosureRequest(dto);
            return APIResponse<IActionResult>.SuccessResponse(null,"ForeClosure Request Added Successfully");
        }

        
        [HttpGet]
        [Route("GetForeClousres")]
        public async Task<APIResponse<List<ForeClosureRequest>>> GetRequests()
        {
            var req=await service.GetForeClosureRequests();
            return APIResponse<List<ForeClosureRequest>>.SuccessResponse(req,"Request Success");
        }

        [HttpPut]
        [Route("UpdateForeClousre")]
        public async Task<APIResponse<IActionResult>> UpdateRequest(int id, string s)
        {
            await service.UpdateForeClosuere(id, s);
            return APIResponse<IActionResult>.SuccessResponse(null,"Updated Successfully");

        }

        [HttpPost]
        [Route("MakeForeClousrePayment")]
        public async Task<APIResponse<IActionResult>> MakeForeClousrePayment(int id)
        {
            var rq = await service.PayForeClosure(id);
            return APIResponse<IActionResult>.SuccessResponse(null,"Payment Done Successfully");
        }
    }
}
    