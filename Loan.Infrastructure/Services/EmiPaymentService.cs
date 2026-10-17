using Loan.Application.DTOs;
using Loan.Application.Interface;
using Loan.Domain.Models;
using Loan.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Razorpay.Api;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Loan.Infrastructure.Services
{
    public class EmiPaymentService : IEmiPaymentService
    {
        AppDbContext db;
        IConfiguration config;
        public EmiPaymentService(AppDbContext db,IConfiguration config)
        {
            this.db= db;
            this.config= config;
        }

        public string CreateRazorpayOrder(CreateOrderDto dto)
        {
            try
            {
                RazorpayClient client = new RazorpayClient(config["Razorpay:Key"],config["Razorpay:Secret"]);
                Dictionary<string, object> options =new Dictionary<string, object>();
                long amountInPaise =Convert.ToInt64(dto.Amount * 100);
                Console.WriteLine($"Amount Received = {dto.Amount}");
                Console.WriteLine($"Amount In Paise = {amountInPaise}");
                options.Add("amount", amountInPaise);
                options.Add("currency", "INR");
                options.Add("receipt", Guid.NewGuid().ToString());
                Order order = client.Order.Create(options);
                Console.WriteLine($"Order Id = {order["id"]}");
                return order["id"].ToString();

            }
            catch (Exception ex)
            {
                Console.WriteLine("RAZORPAY ERROR");
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task<EmiPaymentDetailsDto?> GetEmiPaymentDetails(int id)
        {
            var emi = await db.EmiSchedules.FirstOrDefaultAsync(x => x.EmiScheduleId == id);
            if (emi == null)
                return null;
            var loan = await db.LoanAccounts.FirstOrDefaultAsync(x => x.LoanAccountId == emi.LoanAccountId);
            if (loan == null)
                return null;

            decimal penalty = 0, latefees = 0;
            if (emi.DueDate < DateTime.Now && emi.PaymentStatus == "Pending")
            {
                penalty = 800;
                latefees = 2;
            }
            decimal totalAmount = emi.Emi+penalty+latefees;
            var res = new EmiPaymentDetailsDto
            {

                EmiScheduleId = emi.EmiScheduleId,
                LoanAccountId = emi.LoanAccountId,
                LoanAccountNo = loan.LoanAccountNo,
                DueDate = emi.DueDate,
                EmiAmount = emi.Emi,
                PenaltyAmount = penalty,
                LateFees = latefees,
                TotalPayableAmount = totalAmount,
                PaymentStatus = emi.PaymentStatus

            };
            return res;
        }

        public async Task<string> MakeEmiPayment(CreateEmiPaymentDto dto, string? razorpayPaymentId = null)
        {
            decimal penalty=0,LateFees=0;
            var emi = await db.EmiSchedules.FirstOrDefaultAsync(x => x.EmiScheduleId == dto.EmiScheduleId);
            if (emi == null)
                throw new Exception("Emi not Found");

           if(emi.PaymentStatus =="Paid")
            {
                return "Emi Already Paid";
            }

            if (emi.LoanAccountId != dto.LoanAccountId)
            {
                return "EMI does not belong to this Loan Account";
            }

            var loan = await db.LoanAccounts.FirstOrDefaultAsync(x => x.LoanAccountId == dto.LoanAccountId);
            if(loan == null)
            {
                return "Loan Account Not Found";
            }
            if(loan.LoanStatus!="Active")
            {
                return "Loan is Not Active";
            }

            if (emi.DueDate < DateTime.Now && emi.PaymentStatus == "Pending")
            {
                penalty = 800;
                LateFees = 2;
            }

           decimal totalAmount = emi.Emi + penalty + LateFees;

            var loanpayment = new LoanPayment()
            {
                LoanAccountId = dto.LoanAccountId,
                PaymentName = dto.PaymentMode,
                PaymentDate = DateTime.Now,
                PaidAmount = totalAmount,
                PaymentMode = dto.PaymentMode,
                TransactionReference = razorpayPaymentId ?? Guid.NewGuid().ToString(),
                PrincipalPaid = emi.PrincipalAmount,
                InterestPaid = emi.InterestAmount,
                PenaltyPaid = penalty,
                PaymentStatus = "Success"

            };
            db.LoanPayments.Add(loanpayment);
            emi.PaymentStatus = "Paid";
            emi.PaidDate = DateTime.Now;
            loan.OutstandingPrincipal -= emi.PrincipalAmount;
            loan.TotalPaidAmount += totalAmount;
            await db.SaveChangesAsync();

            return "Emi Paid Successfully";
        }

        public async Task<bool> VerifyEmiPayment(VerifyEmiPaymentDto dtos)
        {
            string secret = config["Razorpay:Secret"];
            string payload= $"{dtos.RazorpayOrderId}|{dtos.RazorpayPaymentId}";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            string generatedSignature = Convert.ToHexString(hash).ToLower();
            bool isValid = generatedSignature.Equals(dtos.RazorpaySignature, StringComparison.OrdinalIgnoreCase);
            RazorpayClient client = new RazorpayClient(config["Razorpay:Key"],config["Razorpay:Secret"]);
            Payment payment = client.Payment.Fetch(dtos.RazorpayPaymentId);
            string status = payment["status"].ToString();
            if (status != "captured")
            {
                return false;
            }
            string orderId = payment["order_id"].ToString();
            if (orderId != dtos.RazorpayOrderId)
            {
                return false;
            }
            var emi = await db.EmiSchedules.FirstOrDefaultAsync(x => x.EmiScheduleId == dtos.EmiScheduleId);
            if (emi == null || emi.PaymentStatus == "Paid")
            {
                return false;
            }
            decimal penalty = 0;
            decimal lateFees = 0;
            if (emi.DueDate < DateTime.Now && emi.PaymentStatus == "Pending")
            {
                penalty = 800;
                lateFees = 2;
            }
            decimal totalAmount = emi.Emi + penalty + lateFees;
            long paidAmount = Convert.ToInt64(payment["amount"]);
            long expectedAmount = Convert.ToInt64(totalAmount * 100);
            if (paidAmount != expectedAmount)
            {
                return false;
            }

            var result = await MakeEmiPayment(new CreateEmiPaymentDto
            {
                LoanAccountId = emi.LoanAccountId,
                EmiScheduleId = emi.EmiScheduleId,
                PaymentMode = dtos.PaymentMode
            },
            dtos.RazorpayPaymentId);
            return result == "Emi Paid Successfully";
        }
    }
}
