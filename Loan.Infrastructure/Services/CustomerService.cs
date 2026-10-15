using Loan.Application.DTOs;
using Loan.Application.Interface;
using Loan.Domain.Models;
using Loan.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Loan.Infrastructure.Services
{
    public class CustomerService : ICustomerService
    {
        AppDbContext db;
        public CustomerService(AppDbContext db)
        {
            this.db = db;
        }
        public Task<List<CustomerDto>> GetAllCustomers()
        {
            var customers = db.Customers.Select(c => new CustomerDto
            {
                Id = c.CustomerId,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Email = c.Email,
                MonthlyIncome = c.MonthlyIncome
            }).ToListAsync();
            return customers;
        }

        public async Task<Customer> GetCustomerById(int id)
        {
            var customer = await db.Customers.FindAsync(id);
            return customer;
        }
    }
}
