
using Loan.Application.DTOs;
using Loan.Application.Interface;
using Loan.Domain.Models;
using Loan.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OtpNet;
using QRCoder;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Loan.Infrastructure.Services
{
    public class UserService : IUserService
    {
        AppDbContext db;
        IEmailService emailService;
        JwtTokenService jwtService;
        IConfiguration config;

        public UserService(AppDbContext db, IEmailService emailService, JwtTokenService jwtService, IConfiguration config)
        {
            this.db = db;
            this.emailService = emailService;
            this.jwtService = jwtService;
            this.config = config;
        }

        public async Task<AuthResponseDto> Register(RegisterDto dto)
        {
            if (await db.Users.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower()))
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Email is already registered."
                };
            }

            if (await db.PendingRegistrations.AnyAsync(p => p.Email.ToLower() == dto.Email.ToLower()))
            {
                db.PendingRegistrations.RemoveRange(
                    db.PendingRegistrations.Where(p => p.Email.ToLower() == dto.Email.ToLower()));
                await db.SaveChangesAsync();
            }

            string otp = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

            PendingRegistration pending = new PendingRegistration
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Age = dto.Age,
                Email = dto.Email,

                PasswordHash = dto.Password,

                MobileNo = dto.MobileNo,
                Pan = dto.Pan,
                AadhaarNo = dto.AadhaarNo,
                EmploymentType = dto.EmploymentType,
                MonthlyIncome = dto.MonthlyIncome,
                TotalMonthlyDebt = dto.TotalMonthlyDebt,

                OtpHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(otp))),
                OtpExpiry = DateTime.UtcNow.AddMinutes(10)
            };

            db.PendingRegistrations.Add(pending);
            await db.SaveChangesAsync();

            try
            {
                await emailService.SendOtp(dto.Email, otp);
            }
            catch
            {
                db.PendingRegistrations.Remove(pending);
                await db.SaveChangesAsync();

                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Unable to send OTP. Please try again."
                };
            }
            return new AuthResponseDto
            {
                Success = true,
                Message = "Registration OTP sent to your email."
            };
        }

        public async Task<AuthResponseDto> VerifyRegistrationOtp(VerifyRegistrationOtpDto dto)
        {
            PendingRegistration? pending = await db.PendingRegistrations.FirstOrDefaultAsync(p => p.Email.ToLower() == dto.Email.ToLower());

            if (pending == null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Registration not found. Please register again."
                };
            }

            if (pending.OtpExpiry < DateTime.UtcNow)
            {
                db.PendingRegistrations.Remove(pending);
                await db.SaveChangesAsync();

                return new AuthResponseDto
                {
                    Success = false,
                    Message = "OTP expired. Please register again."
                };
            }

            string enteredOtpHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(dto.OTP)));

            if (!CryptographicOperations.FixedTimeEquals(
                Convert.FromBase64String(pending.OtpHash),
                Convert.FromBase64String(enteredOtpHash)))
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid registration OTP."
                };
            }

            if (await db.Users.AnyAsync(u => u.Email.ToLower() == pending.Email.ToLower()))
            {
                db.PendingRegistrations.Remove(pending);
                await db.SaveChangesAsync();

                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Email is already registered."
                };
            }

            using var transaction = await db.Database.BeginTransactionAsync();

            try
            {
                Customer customer = new Customer
                {
                    FirstName = pending.FirstName,
                    LastName = pending.LastName,
                    Age = pending.Age,
                    Email = pending.Email,
                    Password = pending.PasswordHash, 
                    MobileNo = pending.MobileNo,
                    Pan = pending.Pan,
                    AadhaarNo = pending.AadhaarNo,
                    EmploymentType = pending.EmploymentType,
                    MonthlyIncome = pending.MonthlyIncome,
                    TotalMonthlyDebt = 0,
                    IsEmailVerified = true
                };

                db.Customers.Add(customer);
                await db.SaveChangesAsync();

                User user = new User
                {
                    FirstName = pending.FirstName,
                    LastName = pending.LastName,
                    Email = pending.Email,
                    Mobile = pending.MobileNo,
                    PasswordHash = pending.PasswordHash,
                    CustomerId = customer.CustomerId,
                    RoleId = 1,
                    IsTwoFactorEnabled = false
                };

                db.Users.Add(user);
                await db.SaveChangesAsync();

                db.PendingRegistrations.Remove(pending);
                await db.SaveChangesAsync();

                await transaction.CommitAsync();

                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Registration successful. You can now log in."
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return new AuthResponseDto
                {
                    Success = false,
                    Message = ex.ToString()
                };

            }
        }

        public async Task<AuthResponseDto> Login(LoginDto dto)
        {
            User? user = await db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());

            if (user == null || user.PasswordHash != dto.Password)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid email or password."
                };
            }

            string challengeId = Guid.NewGuid().ToString("N");

            user.LoginChallengeId = challengeId;
            user.LoginChallengeExpiry = DateTime.UtcNow.AddMinutes(5);
            await db.SaveChangesAsync();

            return new AuthResponseDto
            {
                Success = true,
                Message = user.IsTwoFactorEnabled ? "Enter your authenticator OTP." : "Set up your authenticator app.",
                ChallengeId = challengeId,
                Requires2FASetup = !user.IsTwoFactorEnabled,
                Role = user.Role?.RoleName
            };
        }

        public async Task<AuthResponseDto> Setup2FA(Setup2FADto dto)
        {
            User? user = await db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.LoginChallengeId == dto.ChallengeId && u.LoginChallengeExpiry > DateTime.UtcNow);

            if (user == null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Login challenge expired. Please log in again."
                };
            }

            if (user.IsTwoFactorEnabled)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Two-factor authentication is already enabled."
                };
            }

            if (string.IsNullOrEmpty(user.AuthenticatorSecret))
            {
                user.AuthenticatorSecret = Base32Encoding.ToString(KeyGeneration.GenerateRandomKey(20));

                await db.SaveChangesAsync();
            }

            string issuer = config["Jwt:Issuer"] ?? "LoanApp";

            string otpUrl =
                $"otpauth://totp/{Uri.EscapeDataString(issuer)}:" +
                $"{Uri.EscapeDataString(user.Email)}" +
                $"?secret={user.AuthenticatorSecret}" +
                $"&issuer={Uri.EscapeDataString(issuer)}";

            using QRCodeGenerator generator = new QRCodeGenerator();

            using QRCodeData qrData = generator.CreateQrCode(otpUrl, QRCodeGenerator.ECCLevel.Q);

            PngByteQRCode qrCode = new PngByteQRCode(qrData);

            string base64 = Convert.ToBase64String(
                qrCode.GetGraphic(10));

            return new AuthResponseDto
            {
                Success = true,
                Message = "Scan the QR code using your authenticator app.",
                ChallengeId = dto.ChallengeId,
                QRCode = "data:image/png;base64," + base64,
                Requires2FASetup = true,
                Role = user.Role?.RoleName
            };
        }

        public async Task<AuthResponseDto> VerifyOTP(VerifyOTPDto dto)
        {
            User? user = await db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.LoginChallengeId == dto.ChallengeId && u.LoginChallengeExpiry > DateTime.UtcNow);

            if (user == null || string.IsNullOrEmpty(user.AuthenticatorSecret))
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid challenge or authenticator is not configured."
                };
            }

            Totp totp = new Totp(Base32Encoding.ToBytes(user.AuthenticatorSecret));

            if (!totp.VerifyTotp(dto.OTP, out long matchedStep, new VerificationWindow(1, 1)))
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid authenticator OTP."
                };
            }

            user.IsTwoFactorEnabled = true;
            user.LoginChallengeId = null;
            user.LoginChallengeExpiry = null;

            await db.SaveChangesAsync();

            string token = jwtService.GenerateToken(user);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Login successful.",
                Token = token,
                Role = user.Role?.RoleName
            };
        }
    }
}

