using Loan.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;


namespace Loan.Application.Interface
{
    public interface IScoreCardService
    {
        Task<ScoreCard> GenerateScoreCard(int customerId);
        Task<ScoreCard> GetScoreCard(int customerId);
    }
}
