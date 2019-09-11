using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AuthService.Interface;
using AuthService.Interface.Models;
using Microsoft.AspNetCore.Authentication;

namespace TimeTrackerAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private IAuthService _authService;

        public UsersController(IAuthService authService)
        {
            _authService = authService;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<User> GetUserAsync()
        {
            var token = await this.HttpContext.GetTokenAsync("access_token");
            return await _authService.GetUserAsync(token);
        }
    }
}
