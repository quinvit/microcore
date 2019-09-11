using Hyperscale.Common.Contracts.HttpService;
using ReportService.Interface.Models;
using System;
using System.Threading.Tasks;

namespace ReportService.Interface
{
    [HttpService(80)]
    public interface IReportService
    {
        Task<DailyTimeByUser[]> GetMonthlyReportByUserAsync(string token);
    }
}
