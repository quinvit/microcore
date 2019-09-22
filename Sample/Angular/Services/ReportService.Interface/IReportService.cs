using Hyperscale.Common.Contracts.HttpService;
using ReportService.Interface.Models;
using System;
using System.Threading.Tasks;

namespace ReportService.Interface
{
    [HttpService(80)]
    public interface IReportService
    {
        Task<bool> RegisterTimeRecordsAsync(string token, TimeRecord[] timeRecords);

        Task<DailyTimeByUser[]> GetMonthlyReportByUserAsync(string token, int year);

        Task<TimeRecord[]> GetTimeRecordsReportByUserAsync(string token, DateTime datetime);
    }
}
