using Loan.Application.DTOs;
using Loan.Application.Interface;
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
        public async Task<IActionResult> CreateForeClosureRequest(CreateForeclosureDto dto)
        {
           var res= await service.CreateForeClosureRequest(dto);
            return Ok(res);
        }

        [HttpGet]
        [Route("GetForeClousres")]
        public async Task<IActionResult> GetRequests()
        {
            var req=await service.GetForeClosureRequests();
            return Ok(req);
        }

        [HttpPut]
        [Route("UpdateForeClousre")]
        public async Task<IActionResult> UpdateRequest(int id, string s)
        {
            var req = await service.UpdateForeClosuere(id, s);
            return Ok(req);

        }

        [HttpPost]
        [Route("MakeForeClousrePayment")]
        public async Task<IActionResult> MakeForeClousrePayment(int id)
        {
            var rq = await service.PayForeClosure(id);
            return Ok(rq);
        }
    }
}
