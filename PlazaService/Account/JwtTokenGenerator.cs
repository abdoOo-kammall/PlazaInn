using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PlazaCore.Entites;
using PlazaCore.ServiceContract.Account;

namespace PlazaService.Account
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly IConfiguration _configuration;

        public JwtTokenGenerator(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<string> GenerateToken(ApplicationUser user)
        {

            var claims = new List<Claim>() { 
                new Claim("Id" , user.Id),
                new Claim ("UserName" , user.UserName),
                new Claim ("Email" , user.Email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new Exception("JWT Key not configured")));
            var creds =new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                    claims :claims,
                    signingCredentials:creds,
                     issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"] 
                );
            return  new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
