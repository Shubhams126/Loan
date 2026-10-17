
using Loan.Application.Interface;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Loan.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration configuration;

        public EmailService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

  
        public async Task SendOtp(string email, string otp)
        {
            var smtpHost = configuration["EmailSettings:SmtpHost"];
            var smtpPortValue = configuration["EmailSettings:SmtpPort"];
            var senderEmail = configuration["EmailSettings:SenderEmail"];
            var senderPassword = configuration["EmailSettings:SenderPassword"];

            if (string.IsNullOrWhiteSpace(smtpHost) ||
                !int.TryParse(smtpPortValue, out int smtpPort) ||
                string.IsNullOrWhiteSpace(senderEmail) ||
                string.IsNullOrWhiteSpace(senderPassword))
            {
                throw new InvalidOperationException(
                    "Email settings are missing or invalid.");
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException(
                    "Recipient email address is required.",
                    nameof(email));
            }

            using var message = new MailMessage();

            message.From = new MailAddress(senderEmail);
            message.To.Add(email);
            message.Subject = "FINCORE Loan - Email Verification OTP";
            message.Body =
                $"Your OTP is {otp}. It expires in 10 minutes.";
            message.IsBodyHtml = false;

            using var smtp = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(
                    senderEmail, senderPassword)
            };

            await smtp.SendMailAsync(message);
        }

        public async Task SendEmailWithAttachmentAsync(
            string email,
            string subject,
            string body,
            string filePath,
            string fileName)
        {
            var smtpHost = configuration["EmailSettings:SmtpHost"];
            var smtpPortValue = configuration["EmailSettings:SmtpPort"];
            var senderEmail = configuration["EmailSettings:SenderEmail"];
            var senderPassword = configuration["EmailSettings:SenderPassword"];

            if (string.IsNullOrWhiteSpace(smtpHost) ||
                !int.TryParse(smtpPortValue, out int smtpPort) ||
                string.IsNullOrWhiteSpace(senderEmail) ||
                string.IsNullOrWhiteSpace(senderPassword))
            {
                throw new InvalidOperationException(
                    "Email settings are missing or invalid.");
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException(
                    "Customer email address is required.",
                    nameof(email));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    "Sanction letter PDF was not found.",
                    filePath);
            }

            using var message = new MailMessage();

            message.From = new MailAddress(senderEmail);
            message.To.Add(email);
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = false;

            var pdfStream = File.OpenRead(filePath);

            message.Attachments.Add(
                new Attachment(
                    pdfStream,
                    fileName,
                    "application/pdf"));

            using var smtp = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(
                    senderEmail, senderPassword)
            };

            await smtp.SendMailAsync(message);
        }
    }
}
