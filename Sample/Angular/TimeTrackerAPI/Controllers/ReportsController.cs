using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReportService.Interface;
using ReportService.Interface.Models;

namespace TimeTrackerAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        // GET: api/Reports
        [HttpGet]
        [Route("MonthlyReportByUser")]
        public async Task<DailyTimeByUser[]> GetMonthlyReportByUser()
        {
            var token = await this.HttpContext.GetTokenAsync("access_token");
            return await _reportService.GetMonthlyReportByUserAsync(token);
        }
    }
}
