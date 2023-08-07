using ChatService.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace ChatService.Api.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    public class AccountController : ControllerBase, IResponseWraps
    {
        private static readonly List<dynamic> Users = new()
        {
            new
            {
                Id = 1,
                Mobile = "18530064433",
                UserName = "admin3",
                Password = "admin",
                Role = "admin",
            },
            new
            {
                Id = 2,
                Mobile = "18530064432",
                UserName = "admin2",
                Password = "admin",
                Role = "admin",
            },
            new
            {
                Id = 3,
                Mobile = "18530064431",
                UserName = "sa",
                Password = "admin",
                Role = "SystemAdmin",
            }
        };

        private string GetRasPublishKey()
        {
            using (var rsa=RSA.Create())
            {
                var publishKey=rsa.ExportRSAPublicKey();
                return Convert.ToBase64String(publishKey);
            }
        }


        /// <summary>
        /// 获取jwt token
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<ResultDto> SignInAsync([Required, FromForm(Name = "userName")] string user, [Required, FromForm(Name = "password")] string pwd)
        {
            var u = Users.FirstOrDefault(x => x.UserName == user && x.Password == pwd);
            if (u == null)
            {
                return this.ErrorRequest("用户不存在");
            }
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, u.UserName),
                new Claim(ClaimTypes.NameIdentifier, u.Id),
                new Claim(ClaimTypes.Role, u.Role),
            };
            var key = new SymmetricSecurityKey("715B59F3CDB1CF8BC3E7C8F13794CEA9"u8.ToArray());
            var token = new JwtSecurityToken(
                issuer: "pluto",
                audience: "123",
                notBefore: DateTime.Now,
                expires: DateTime.Now.AddSeconds(120),
                claims: claims,
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );
            return await Task.FromResult(this.Success(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token)
            }));
        }



    }
}