
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

                PasswordHash = new Microsoft.AspNetCore.Identity.PasswordHasher<User>()
                    .HashPassword(new User(), dto.Password),

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
            if (string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Email and password are required."
                };
            }

            var user = await db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u =>
                    u.Email.ToLower() == dto.Email.Trim().ToLower());

            if (user == null || string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid email or password."
                };
            }

            var passwordHasher =
                new Microsoft.AspNetCore.Identity.PasswordHasher<User>();

            bool passwordIsValid = false;

            // Verify the password hash.
            try
            {
                var result = passwordHasher.VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    dto.Password);

                passwordIsValid =
                    result != Microsoft.AspNetCore.Identity
                        .PasswordVerificationResult.Failed;

                if (result ==
                    Microsoft.AspNetCore.Identity
                        .PasswordVerificationResult.SuccessRehashNeeded)
                {
                    user.PasswordHash =
                        passwordHasher.HashPassword(user, dto.Password);
                }
            }
            catch (FormatException)
            {
                passwordIsValid = false;
            }

            if (!passwordIsValid)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid email or password."
                };
            }

            // Generate a fresh challenge after successful password validation.
            string challengeId = Guid.NewGuid().ToString("N");

            user.LoginChallengeId = challengeId;
            user.LoginChallengeExpiry = DateTime.UtcNow.AddMinutes(5);

            // Save the challenge before returning it.
            await db.SaveChangesAsync();

            bool requiresSetup =
                !user.IsTwoFactorEnabled ||
                string.IsNullOrWhiteSpace(user.AuthenticatorSecret);

            Console.WriteLine($"LOGIN METHOD HIT - ChallengeId: {challengeId}");

            // Return the generated value directly.
            return new AuthResponseDto
            {
                Success = true,
                Message = "Password verified. Enter your authenticator OTP.",
                ChallengeId = challengeId,
                Requires2FASetup = requiresSetup,
                Role = user.Role?.RoleName
            };
        }

        public async Task<AuthResponseDto> Setup2FA(Setup2FADto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.ChallengeId))
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Challenge ID is missing. Please log in again."
                };
            }

            var user = await db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u =>
                    u.LoginChallengeId == dto.ChallengeId);

            if (user == null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Challenge not found. Please log in again."
                };
            }

            if (user.LoginChallengeExpiry == null ||
                user.LoginChallengeExpiry <= DateTime.UtcNow)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Login challenge expired. Please log in again."
                };
            }

            if (user.IsTwoFactorEnabled &&
                !string.IsNullOrWhiteSpace(user.AuthenticatorSecret))
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Two-factor authentication is already enabled."
                };
            }

            // Generate a secret only if one does not already exist.
            if (string.IsNullOrWhiteSpace(user.AuthenticatorSecret))
            {
                user.AuthenticatorSecret =
                    OtpNet.Base32Encoding.ToString(
                        OtpNet.KeyGeneration.GenerateRandomKey(20));
            }

            string issuer = config["Jwt:Issuer"] ?? "LoanApp";

            string otpUrl =
                $"otpauth://totp/{Uri.EscapeDataString(issuer)}:" +
                $"{Uri.EscapeDataString(user.Email)}" +
                $"?secret={user.AuthenticatorSecret}" +
                $"&issuer={Uri.EscapeDataString(issuer)}";

            using var generator = new QRCoder.QRCodeGenerator();

            using var qrData = generator.CreateQrCode(
                otpUrl,
                QRCoder.QRCodeGenerator.ECCLevel.Q);

            var qrCode = new QRCoder.PngByteQRCode(qrData);

            string base64 = Convert.ToBase64String(qrCode.GetGraphic(10));

            await db.SaveChangesAsync();

            return new AuthResponseDto
            {
                Success = true,
                Message = "Scan the QR code and enter the authenticator code.",
                ChallengeId = user.LoginChallengeId,
                Requires2FASetup = true,
                QRCode = "data:image/png;base64," + base64,
                Role = user.Role?.RoleName
            };
        }

        public async Task<AuthResponseDto> VerifyOTP(VerifyOTPDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.ChallengeId) ||
                string.IsNullOrWhiteSpace(dto.OTP))
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Challenge ID and OTP are required."
                };
            }

            var user = await db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u =>
                    u.LoginChallengeId == dto.ChallengeId);

            if (user == null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Challenge not found. Please log in again."
                };
            }

            if (user.LoginChallengeExpiry == null ||
                user.LoginChallengeExpiry <= DateTime.UtcNow)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Login challenge expired. Please log in again."
                };
            }

            if (string.IsNullOrWhiteSpace(user.AuthenticatorSecret))
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Authenticator is not configured. Please set it up."
                };
            }

            bool validOtp;

            try
            {
                var totp = new OtpNet.Totp(
                    OtpNet.Base32Encoding.ToBytes(user.AuthenticatorSecret));

                validOtp = totp.VerifyTotp(
                    dto.OTP.Trim(),
                    out _,
                    new OtpNet.VerificationWindow(previous: 1, future: 1));
            }
            catch (FormatException)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Authenticator secret is invalid."
                };
            }

            if (!validOtp)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid authenticator OTP. Try the current code."
                };
            }

            // OTP is valid.
            user.IsTwoFactorEnabled = true;

            // Invalidate the challenge after successful verification.
            user.LoginChallengeId = null;
            user.LoginChallengeExpiry = null;

            await db.SaveChangesAsync();

            // Use your existing JWT service.
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

