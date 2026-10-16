using System;
using System.Collections.Generic;
using System.Text;

namespace Loan.Application.Interface
{
    public interface IEmailService
    {
        Task SendEmailWithAttachmentAsync(string email, string v1, string v2, string filePath, string fileName);
        Task SendOtp(string email, string otp);
    }
}
