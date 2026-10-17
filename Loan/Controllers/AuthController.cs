using Loan.Application.DTOs;
using Loan.Application.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Loan.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        IUserService service;

        public AuthController(IUserService service)
        {
            this.service = service;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var result = await service.Register(dto);
            return Ok(result);
        }

        [HttpPost]
        [Route("VerifyRegistrationOtp")]
        public async Task<IActionResult> VerifyRegistrationOtp(VerifyRegistrationOtpDto dto)
        {
            var result = await service.VerifyRegistrationOtp(dto);
            return Ok(result);
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var result = await service.Login(dto);
            return Ok(result);
        }

        [HttpPost]
        [Route("Setup2FA")]
        public async Task<IActionResult> Setup2FA(Setup2FADto dto)
        {
            var result = await service.Setup2FA(dto);
            return Ok(result);
        }

        [HttpPost]
        [Route("VerifyOTP")]
        public async Task<IActionResult> VerifyOTP(VerifyOTPDto dto)
        {
            var result = await service.VerifyOTP(dto);
            return Ok(result);
        }
        [HttpPost]
        [Route("GoogleLogin")]
        public async Task<IActionResult> GoogleLogin(GoogleLoginDto dto)
        {
            var result = await service.GoogleLogin(dto);
            return Ok(result);
        }
    }
}