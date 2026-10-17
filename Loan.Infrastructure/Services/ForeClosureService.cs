using Azure.Core;
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
    public class ForeClosureService : IForeClosureService
    {
        AppDbContext db;
        IConfiguration config;
        private readonly EmiPaymentService emiPaymentService;
        public ForeClosureService(AppDbContext db, EmiPaymentService emiPaymentService, IConfiguration config)
        {
            this.db = db;
            this.emiPaymentService = emiPaymentService;
            this.config = config;
        }
        public async Task<string> CreateForeClosureRequest(CreateForeclosureDto dto)
        {
           var loan = await db.LoanAccounts.FirstOrDefaultAsync(x=>x.LoanAccountId == dto.LoanAccountId);
            if(loan== null)
            {
                return "Loan Account Not Found";
            }
            if(loan.LoanStatus != "Active")
            {
                return "Loan is not Active";
            }
            if(dto.ForeClosureType !="Full" && dto.ForeClosureType != "Partial")
            {
                return "Invalid ForeClosure Type";
            }
            if (dto.ForeClosureType == "Partial" && dto.PartialAmount <= 0)
            {
                return "Amount Must Be Greater then 0";
            } else if (dto.ForeClosureType == "Partial" && dto.PartialAmount > loan.OutstandingPrincipal)
            {
                return "Amount Must not be greater then Loan Amount";
            }

            var req = new ForeClosureRequest()
            {
                LoanAccountId = dto.LoanAccountId,
                ForeClosureType = dto.ForeClosureType,
                ForeClosureAmount = loan.OutstandingPrincipal,
                PartialAmount = dto.PartialAmount,
                RequestedDate = DateTime.Now,
                ExpectedClosureDate = DateTime.Now,
                Reason = dto.Reason,
                Status = "Pending",
                IsPaid = false
            };
            db.ForeClosureRequests.Add(req);

            await db.SaveChangesAsync();
            return "ForeClosure Request Created Successfully";
        }

        public async Task<List<ForeClosureRequestDto>> GetForeClosureRequests()
        {
            var reqs = await db.ForeClosureRequests.Include(x => x.LoanAccount).ThenInclude(x => x.Customer)
                .Select(x => new ForeClosureRequestDto
                {
                    RequestId = x.RequestId,
                    LoanAccountId = x.LoanAccountId,
                    LoanAccountNo = x.LoanAccount!.LoanAccountNo,
                    CustomerName = x.LoanAccount!.Customer!.FirstName,
                    ForeClosureType = x.ForeClosureType,
                    RequestedDate = x.RequestedDate,
                    ForeClosureAmount = x.ForeClosureAmount,
                    OutstandingPrincipal = x.LoanAccount.OutstandingPrincipal,
                    Status = x.Status,
                    RequestCategory = "Foreclosure"
                }).ToListAsync();
                
            return reqs;
        }

        public async Task<List<ForeClosureRequest>> GetForeClosureRequestbyid(int id)
        {
            var req = await db.ForeClosureRequests.Where(x => x.LoanAccount!.CustomerId == id).ToListAsync();
            return req;
        }

        public async Task<string> PayForeClosure(int rid)
        {
            var res = await db.ForeClosureRequests.FirstOrDefaultAsync(x => x.RequestId == rid && x.Status == "Approved");
            if(res == null)
            {
                return "Request Not Found ";
            }
            if(res.Status!="Approved")
            {
                return "Request is not Aprroved";
            }
            var loan = await db.LoanAccounts.FirstOrDefaultAsync(x=>x.LoanAccountId == res.LoanAccountId);
            if (loan.LoanStatus != "Active")
                return "Loan is not Active";

            if(res.ForeClosureType == "Full")
            {
                loan.OutstandingPrincipal = 0;
                loan.LoanStatus = "Loan Closed";
            }
            else if(res.ForeClosureType == "Partial")
            {

                if (res.PartialAmount <= 0 ||res.PartialAmount >= loan.OutstandingPrincipal)
                {
                    return "Invalid Partial Amount";
                }
                loan.OutstandingPrincipal -=res.PartialAmount;
                loan.TotalPaidAmount += res.PartialAmount;
            }
            else
            {
                return "Invalid ClosureType";
            }
            res.IsPaid = true;
            res.PaidDate = DateTime.Now;
            

            if(res.ForeClosureType =="Full")
            {
                var lc = new LoanClosure()
                {
                    LoanAccountId = loan.LoanAccountId,
                    ClosureType = "Full",
                    FinalSettlementAmount = res.ForeClosureAmount,
                    ClosureDate = DateTime.Now,
                    ClosedBy = 2,
                    Remarks = res.Reason,
                    ClosureStatus = "Closed"
                };
                db.LoanClosures.Add(lc);
            }
            else if(res.ForeClosureType =="Partial")
            {
                var lc = new LoanClosure()
                {
                    LoanAccountId = loan.LoanAccountId,
                    ClosureType = "Partial",
                    FinalSettlementAmount = res.PartialAmount,
                    ClosureDate = DateTime.Now,
                    ClosedBy = 2,
                    Remarks = res.Reason,
                    ClosureStatus = "Closed"
                };
                db.LoanClosures.Add(lc);
            }
            await db.SaveChangesAsync();

            return "ForeClosure Payment Processed Successfully";

        }

        public async Task<string> CreateForeClosureOrder(int requestId)
        {
            var request = await db.ForeClosureRequests
                .FirstOrDefaultAsync(x => x.RequestId == requestId);

            if (request == null)
                return "Request Not Found";

            if (request.Status != "Approved")
                return "Request is not Approved";
            if (request.IsPaid)
                return "Payment Already Done";
            decimal amount = request.ForeClosureType == "Full"
                ? request.ForeClosureAmount
                : request.PartialAmount;
            return emiPaymentService.CreateRazorpayOrder(
                    new CreateOrderDto
                    {
                     Amount = amount
                     });
        }

        public async Task<string> UpdateForeClosuere(int rid, UpdateForeClosureDto dto)
        {
           var res=  await db.ForeClosureRequests.FindAsync(rid);
            if(res== null)
            {
                return "Invalid Request";
            }
            if(res.Status!="Pending")
            {
                return "Request Already Processd";

            }
            res.Status = dto.Status;
            await db.SaveChangesAsync();
            return $"{dto.Status} Successfully";
        }

        public async Task<ForeClosureRequestDto> GetForeClosure(int id)
        {
            var reqs = await db.ForeClosureRequests.Include(x => x.LoanAccount).ThenInclude(x => x.Customer)
                .Select(x => new ForeClosureRequestDto
                {
                    RequestId = x.RequestId,
                    LoanAccountId = x.LoanAccountId,
                    LoanAccountNo = x.LoanAccount!.LoanAccountNo,
                    CustomerName = x.LoanAccount!.Customer!.FirstName,
                    ForeClosureType = x.ForeClosureType,
                    RequestedDate = x.RequestedDate,
                    ForeClosureAmount = x.ForeClosureAmount,
                    OutstandingPrincipal = x.LoanAccount.OutstandingPrincipal,
                    Status = x.Status,
                    RequestCategory = "Foreclosure"
                }).FirstOrDefaultAsync(x => x.RequestId == id);
            return reqs;
        }

        public async Task<bool> VerifyForeClosurePayment(VerifyForeClosurePaymentDto dto)
        {
            string secret = config["Razorpay:Secret"];
            string payload = $"{dto.RazorpayOrderId}|{dto.RazorpayPaymentId}";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            string generatedSignature = Convert.ToHexString(hash).ToLower();
            if (!generatedSignature.Equals(dto.RazorpaySignature,StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            RazorpayClient client = new RazorpayClient(config["Razorpay:Key"],config["Razorpay:Secret"]);
            Payment payment = client.Payment.Fetch(dto.RazorpayPaymentId);
            if (payment["status"].ToString() != "captured")
                return false;
            if (payment["order_id"].ToString() != dto.RazorpayOrderId)
                return false;
            var request = await db.ForeClosureRequests.FirstOrDefaultAsync(x => x.RequestId == dto.RequestId);
            if (request == null || request.Status != "Approved" ||
                request.IsPaid)
            {
                return false;
            }
            var loan = await db.LoanAccounts.FirstOrDefaultAsync(x =>x.LoanAccountId == request.LoanAccountId);
            if (loan == null || loan.LoanStatus != "Active")
                return false;
            decimal amount = request.ForeClosureType == "Full"? request.ForeClosureAmount: request.PartialAmount;
            long expectedAmount = Convert.ToInt64(amount * 100);
            long paidAmount = Convert.ToInt64(payment["amount"]);
            if (paidAmount != expectedAmount)
                return false;
            string result = await PayForeClosure(dto.RequestId);
            return result == "ForeClosure Payment Processed Successfully";
        }
    }
}
