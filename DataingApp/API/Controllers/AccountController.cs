using API.Data;
using API.Dtos;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace API.Controllers
{
    public class AccountController: BaseApiController
    {
        public readonly DataContext _dataContext;
        private readonly ITokenService tokenService;

        public AccountController(DataContext dataContext,ITokenService tokenService)
        {
            this._dataContext = dataContext;
            this.tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AppUser>> Register([FromBody]RegisterDto register)
        {
            var userexist = await _dataContext.User.AnyAsync(x => x.UserName == register.UserName.ToLower());
            if (userexist)
            {
                return BadRequest();
            }
            var hmac = new HMACSHA512();
            var user = new AppUser
            {
                UserName = register.UserName.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(register.Password)),
                PasswordSalt = hmac.Key
            };

            _dataContext.Add(user);
            await _dataContext.SaveChangesAsync();
            return user;
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto login)
        {
            var user = await _dataContext.User.SingleAsync(x => x.UserName == login.Name.ToLower());
            if (user == null)
            {
                return Unauthorized();
            }

            var hmac = new HMACSHA512(user.PasswordSalt);
            var computedPasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(login.Password));

            for(int i = 0; i < computedPasswordHash.Length; i++)
            {
                if (user.PasswordHash[i] != computedPasswordHash[i])
                {
                    return Unauthorized();
                }
            }

            return new UserDto
            {
                UserName = user.UserName,
                Token = tokenService.CreateToken(user)
            };
        }
    }
}
