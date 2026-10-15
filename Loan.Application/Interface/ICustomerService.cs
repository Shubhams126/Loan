using Loan.Domain.Models;
using Loan.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Loan.Application.Interface
{
    public interface ICustomerService
    {
        Task<List<CustomerDto>> GetAllCustomers();
        Task<Customer> GetCustomerById(int id);
    }
}
