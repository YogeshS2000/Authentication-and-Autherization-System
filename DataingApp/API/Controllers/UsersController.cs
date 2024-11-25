using API.Data;
using API.Entities;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Dfareporting.v4;
using Google.Apis.Util.Store;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        public readonly DataContext _dataContext;

        public UsersController(DataContext dataContext)
        {
            this._dataContext = dataContext;
        }

        [HttpGet]
        public async Task<List<AppUser>> Get()
        {
            var users = await _dataContext.User.ToListAsync();
            return users;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AppUser>> GetUser(int id)
        {
            var user = await _dataContext.User.FindAsync(id);

            return Ok(user);
        }
        [HttpPost]
        public async Task<IActionResult> AddUser(AppUser user)
        {
            var newUser = new AppUser
            {
                UserName = user.UserName,
            };
             await _dataContext.User.AddAsync(newUser);
            var result =  await _dataContext.SaveChangesAsync();

            return Ok(result);
        }

    }
}
