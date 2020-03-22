using AuthService.Interface;
using AuthService.Storages;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using ReportService.Entities;
using ReportService.Interface;
using ReportService.Interface.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReportService
{
    public class ReportService : IReportService
    {
        public const string TimeRecordTableName = "TimeRecords";

        private const int MinYear = 2010;

        private readonly IConfiguration _configuration;
        private readonly IAuthService _authService;

        public ReportService(IConfiguration configuration, IAuthService authService)
        {
            _configuration = configuration;
            _authService = authService;
        }

        private string FormatDuration(int number)
        {
            return string.Concat("[", number.ToString(), "]");
        }

        public async Task<DailyTimeByUser[]> GetMonthlyReportByUserAsync(string token, int year)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentNullException("token");
            }

            if(year < MinYear)
            {
                throw new ArgumentException("Cannot get data from year sooner than 2010.");
            }

            var user = await _authService.GetUserAsync(token);
            if(user == null)
            {
                return new DailyTimeByUser[] { };
            }

            var username = user.Username;

            // yyyy'-'MM'-'dd HH':'mm':'ss'Z'
            var startRowKey = DateTime.Parse($"{year}-01-01 00:00:00").Ticks.ToString();
            var endRowKey = DateTime.Parse($"{year}-12-31 23:59:59").Ticks.ToString();

            try
            {
                var table = await TableStorage.CreateTableAsync(_configuration, TimeRecordTableName);

                // Create the range query using the fluid API 
                TableQuery<TimeRecordEntity> rangeQuery = new TableQuery<TimeRecordEntity>().Where(
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, username),
                        TableOperators.And,
                        TableQuery.CombineFilters(
                            TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual,
                                startRowKey),
                            TableOperators.And,
                            TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThanOrEqual,
                                endRowKey))));

                var entities = await table.ExecuteQueryAsync(rangeQuery);
                var records = entities
                .GroupBy(x => $"{x.RecordedTime?.Month}-{x.RecordedTime?.Day}")
                .Select(g => new DailyTimeByUser
                {
                    Description = string.Join(" | ", g.Select(x => x.Description + " " + FormatDuration(x.TotalMinutes))),
                    TotalMinutes = g.Sum(r => r.TotalMinutes),
                    DayInMonth = g.First().RecordedTime.Value.Day,
                    MonthInYear = g.First().RecordedTime.Value.Month
                }).OrderByDescending(x => x.DayInMonth).ToArray();

                return records;
            }
            catch (StorageException e)
            {
                Log.Logger.Error(e, "Error when query data from storage.");
            }

            return new DailyTimeByUser[] { };
        }

        public async Task<TimeRecord[]> GetTimeRecordsReportByUserAsync(string token, DateTime datetime)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentNullException("token");
            }

            var now = DateTime.UtcNow;

            if (datetime < now.AddDays(-30))
            {
                throw new ArgumentException("Cannot get data later than 30 days.");
            }

            var user = await _authService.GetUserAsync(token);
            if (user == null)
            {
                return new TimeRecord[] { };
            }

            var username = user.Username;
            // yyyy'-'MM'-'dd HH':'mm':'ss'Z'
            var startRowKey = DateTime.Parse($"{datetime.Year}-{datetime.Month.ToString("D2")}-{datetime.Day.ToString("D2")} 00:00:00")
                .Ticks.ToString();
            var endRowKey = DateTime.Parse($"{datetime.Year}-{datetime.Month.ToString("D2")}-{datetime.Day.ToString("D2")} 23:59:59")
                .Ticks.ToString();

            try
            {
                var table = await TableStorage.CreateTableAsync(_configuration, TimeRecordTableName);

                // Create the range query using the fluid API 
                TableQuery<TimeRecordEntity> rangeQuery = new TableQuery<TimeRecordEntity>().Where(
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, username),
                        TableOperators.And,
                        TableQuery.CombineFilters(
                            TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual,
                                startRowKey),
                            TableOperators.And,
                            TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThanOrEqual,
                                endRowKey))));

                var entities = await table.ExecuteQueryAsync(rangeQuery);
                var records = entities
                .Select(g => new TimeRecord
                {
                    RecordedTime = g.RecordedTime,
                    ModifiedTime = g.ModifiedTime,
                    Description = g.Description,
                    TotalMinutes = g.TotalMinutes
                }).ToArray();

                return records;
            }
            catch (StorageException e)
            {
                Log.Logger.Error(e, "Error when query data from storage.");
            }

            return new TimeRecord[] { };
        }

        public async Task<bool> RegisterTimeRecordsAsync(string token, TimeRecord[] timeRecords)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentNullException("token");
            }

            if (timeRecords == null)
            {
                throw new ArgumentNullException("timeRecords");
            }

            var user = await _authService.GetUserAsync(token);

            if (user == null)
            {
                Log.Logger.Error("Cannot find user with provided token.");
                return false;
            }

            var username = user.Username;

            try
            {
                var table = await TableStorage.CreateTableAsync(_configuration, TimeRecordTableName);
                TableBatchOperation batch = new TableBatchOperation();

                foreach (var timeRecord in timeRecords)
                {
                    timeRecord.RecordedBy = username;

                    if (!timeRecord.RecordedTime.HasValue)
                    {
                        timeRecord.RecordedTime = DateTime.UtcNow;
                    }

                    timeRecord.ModifiedTime = DateTime.UtcNow;
                    var entity = new TimeRecordEntity(timeRecord);
                    batch.InsertOrMerge(entity);
                }

                await table.ExecuteBatchAsync(batch);
                return true;
            }
            catch (StorageException e)
            {
                Log.Logger.Error(e, "Error when insert/merge time record to storage.");
            }

            return false;
        }
    }
}
