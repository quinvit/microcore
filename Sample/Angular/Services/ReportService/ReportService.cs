using AuthService.Interface;
using ReportService.Interface;
using ReportService.Interface.Models;
using System.Threading.Tasks;

namespace ReportService
{
    public class ReportService : IReportService
    {
        private IAuthService _authService;

        public ReportService(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<DailyTimeByUser[]> GetMonthlyReportByUserAsync(string token)
        {
            var user = await _authService.GetUserAsync(token);
            var fullName = $"{user.FirstName} {user.LastName}";

            var records = new DailyTimeByUser[] 
            {
                new DailyTimeByUser() { Name = fullName, DayInMonth = 1, TotalMinutes = 420, Email = user.Email },
                new DailyTimeByUser() { Name = fullName, DayInMonth = 2, TotalMinutes = 500, Email = user.Email }
            };

            return await Task.FromResult(records);
        }
    }
}
