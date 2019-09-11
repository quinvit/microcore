using AuthService.Interface;
using AuthService.Interface.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using System.Linq;

namespace AuthService
{
    public class AuthService : IAuthService
    {
        public async Task<User> GetUserAsync(string token)
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            var claims = jwt.Claims.ToList();

            var user = new User()
            {
                Username = claims.FirstOrDefault(x => x.Type == "unique_name")?.Value,
                Email = claims.FirstOrDefault(x => x.Type == "email")?.Value,
                FirstName = claims.FirstOrDefault(x => x.Type == "family_name")?.Value,
                LastName = claims.FirstOrDefault(x => x.Type == "given_name")?.Value
            };

            return await Task.FromResult(user);
        }
    }
}
