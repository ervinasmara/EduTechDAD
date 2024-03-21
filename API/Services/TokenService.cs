﻿using Domain.User;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.Services
{
    public class TokenService
    {
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config)
        {
            _config = config;
        }
        public string CreateTokenAdmin1(AppUser user, Teacher teacher)
        {
            // Kita akan membuat daftar klaim yang akan masuk ke dalam dan dikembalikan dengan token kita
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim("NameTeacher", teacher.NameTeacher),
                new Claim("BirthDate", teacher.BirthDate.ToString()),
                new Claim("BirthPlace", teacher.BirthPlace),
                new Claim("Address", teacher.Address),
                new Claim("PhoneNumber", teacher.PhoneNumber),
                new Claim("Nip", teacher.Nip),
            };

            // Dan kita perlu menggunakan kunci keamanan simetris
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["TokenKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature); // ini akan digunakan untuk menandatangani key

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public string CreateTokenAdmin(AppUser user, Admin admin)
        {
            // Kita akan membuat daftar klaim yang akan masuk ke dalam dan dikembalikan dengan token kita
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim("NameAdmin", admin.NameAdmin),
            };

            // Dan kita perlu menggunakan kunci keamanan simetris
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["TokenKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature); // ini akan digunakan untuk menandatangani key

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}