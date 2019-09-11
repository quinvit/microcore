using AuthService.Interface.Models;
using Hyperscale.Common.Contracts.HttpService;
using System;
using System.Threading.Tasks;

namespace AuthService.Interface
{
    [HttpService(80)]
    public interface IAuthService
    {
        Task<User> GetUserAsync(string token);
    }
}
