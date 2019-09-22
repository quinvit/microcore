using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AuthService.Interface;
using AuthService.Interface.Models;
using Microsoft.AspNetCore.Authentication;

namespace TimeTrackerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IAuthService _authService;

        public UsersController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost]
        public async Task<bool> RegisterUserAsync(User user)
        {
            return await _authService.RegisterUserAsync(user);
        }

        [Authorize]
        [HttpGet]
        public async Task<User> GetUserAsync()
        {
            var token = await this.HttpContext.GetTokenAsync("access_token");
            return await _authService.GetUserAsync(token);
        }
    }
}
