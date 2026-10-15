using Loan.Application.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Loan.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        ICustomerService service;
        public CustomerController(ICustomerService service)
        {
            this.service = service;
        }

        [HttpGet]
        [Route("api/[controller]/GetAllCustomers")]
        public async Task<IActionResult> GetAllCustomers()
        {
            var customers = await service.GetAllCustomers();
            return Ok(customers);
        }

        [HttpGet]
        [Route("api/[controller]/GetCustomerById/{id}")]
        public async Task<IActionResult> GetCustomerById(int id)
        {
            var customer = await service.GetCustomerById(id);
            if (customer == null)
            {
                return NotFound("Customer not found");
            }
            return Ok(customer);
        }
    }
}
