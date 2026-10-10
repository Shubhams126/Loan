using Loan.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Loan.Domain.Models;

namespace Loan.Application.Interface
{
    public interface IScoreCardService
    {
        Task<ScoreCard> GenerateScoreCard(int customerId);
        Task<ScoreCard> GetScoreCard(int customerId);
    }
}
