using Loan.Application.Interface;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Loan.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        IConfiguration configuration;

        public EmailService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task SendOtp(string email, string otp)
        {
            var smtpHost = configuration["EmailSettings:SmtpHost"];
            var smtpPort = int.Parse(configuration["EmailSettings:SmtpPort"]!);
            var senderEmail = configuration["EmailSettings:SenderEmail"];
            var senderPassword = configuration["EmailSettings:SenderPassword"];

            using var message = new MailMessage();
            message.From = new MailAddress(senderEmail!);
            message.To.Add(email);
            message.Subject = "Loan App - Email Verification OTP";
            message.Body = $"Your OTP is {otp}. It expires in 10 minutes.";
            message.IsBodyHtml = false;

            using var smtp = new SmtpClient(smtpHost, smtpPort);
            smtp.EnableSsl = true;
            smtp.Credentials = new NetworkCredential(senderEmail, senderPassword);

            await smtp.SendMailAsync(message);
        }
    }
}
