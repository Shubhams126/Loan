using Azure.Core;
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
    public class ForeClosureService : IForeClosureService
    {
        AppDbContext db;
        public ForeClosureService(AppDbContext db)
        {
            this.db = db;
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

        public async Task<List<ForeClosureRequest>> GetForeClosureRequests()
        {
            var reqs = await db.ForeClosureRequests.ToListAsync();
            return reqs;
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

        public async Task<string> UpdateForeClosuere(int rid, string s)
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
            res.Status = s;
            await db.SaveChangesAsync();
            return $"{s} Successfully";
        }
    }
}
