using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReportService.Interface;
using ReportService.Interface.Models;
using TimeTrackerAPI.Models;

namespace TimeTrackerAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpPost]
        [Route("Keyin")]
        public async Task<Result> RegisterTimeRecord(TimeRecord[] timeRecords)
        {
            if (timeRecords == null || timeRecords.Length == 0)
            {
                return new Result(-1, "timeRecords cannot be null or empty.");
            }
            
            var token = await this.HttpContext.GetTokenAsync("access_token");
            var success = await _reportService.RegisterTimeRecordsAsync(token, 
                timeRecords.Where(x => !string.IsNullOrEmpty(x.Description) && x.TotalMinutes != 0).ToArray()
                );

            return success ? new Result(0, null) : new Result(1, "Cannot keyin this time. Please try again later or contact your manager.");
        }

        [HttpGet]
        [Route("MonthlyReportByUser")]
        public async Task<DailyTimeByUser[]> GetMonthlyReportByUser()
        {
            var token = await this.HttpContext.GetTokenAsync("access_token");
            return await _reportService.GetMonthlyReportByUserAsync(token, DateTime.UtcNow.Year);
        }

        [HttpGet]
        [Route("DailyReportByUser")]
        public async Task<TimeRecord[]> GetDailyReportByUser(DateTime date)
        {
            var token = await this.HttpContext.GetTokenAsync("access_token");
            return await _reportService.GetTimeRecordsReportByUserAsync(token, date.ToUniversalTime());
        }
    }
}
