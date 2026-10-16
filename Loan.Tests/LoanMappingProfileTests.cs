
using AutoMapper;
using Loan.Application.Mapping;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Loan.Tests
{
    public class LoanMappingProfileTests
    {
        [Fact]
        public void MappingProfile_ShouldBeValid()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<LoanMappingProfile>();
            }, NullLoggerFactory.Instance);

            config.AssertConfigurationIsValid();
        }
    }
}
