using Loan.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Loan.Infrastructure.Services
{
    public class JwtTokenService
    {
        IConfiguration config;
        public JwtTokenService(IConfiguration config)
        {
            this.config = config;
        }
        public string GenerateToken(User user)
        {
            string key = config["Jwt:Key"] ?? throw new InvalidOperationException("JWT key is missing.");

            string issuer = config["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT issuer is missing.");

            string audience = config["Jwt:Audience"] ?? throw new InvalidOperationException("JWT audience is missing.");

            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role?.RoleName ?? "")
            };

            if (user.CustomerId.HasValue)
            {
                claims.Add(new Claim("CustomerId", user.CustomerId.Value.ToString()));

            }

            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

            SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken token = new JwtSecurityToken(issuer, audience, claims, expires: DateTime.UtcNow.AddHours(1), signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
