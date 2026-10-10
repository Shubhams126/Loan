using Loan.Application.DTOs;
using Loan.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Loan.Application.Interface
{
    public interface IUserService
    {
        Task<AuthResponseDto> Register(RegisterDto dto);

        Task<AuthResponseDto> VerifyRegistrationOtp(VerifyRegistrationOtpDto dto);


        Task<AuthResponseDto> Login(LoginDto dto);

        Task<AuthResponseDto> Setup2FA(Setup2FADto dto);

        Task<AuthResponseDto> VerifyOTP(VerifyOTPDto dto);

        //Task<AuthResponseDto> GoogleLogin(GoogleLoginDto dto);
    }
}
