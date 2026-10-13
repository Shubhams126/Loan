using Loan.Domain.Models;

namespace Loan.Application.Interface
{
    public interface ICibilService
    {
        Task<CibilReport> GetCibilScore(Customer customer);
        Task<CibilReport?> GetCibilScore(int customerId);
        Task<CibilReport?> GetLatestCibil(int customerId);
    }
}
