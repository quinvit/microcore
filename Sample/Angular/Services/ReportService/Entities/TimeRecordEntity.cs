using Microsoft.Azure.Cosmos.Table;
using ReportService.Interface.Models;
using System;

namespace ReportService.Entities
{
    public class TimeRecordEntity : TableEntity
    {
        public int TotalMinutes { get; set; }

        public string Description { get; set; }

        public string RecordedBy { get; set; }

        public DateTime? RecordedTime { get; set; }

        public string ModifiedBy { get; set; }

        public DateTime? ModifiedTime { get; set; }

        public TimeRecordEntity()
        {
        }

        public TimeRecordEntity(TimeRecord timeRecord) : this(timeRecord.RecordedBy, timeRecord.RecordedTime)
        {
            RecordedTime = timeRecord.RecordedTime;
            RecordedBy = timeRecord.RecordedBy;
            ModifiedTime = timeRecord.ModifiedTime;
            ModifiedBy = timeRecord.ModifiedBy;
            TotalMinutes = timeRecord.TotalMinutes;
            Description = timeRecord.Description;
        }

        public TimeRecordEntity(string recordedBy, DateTime? recordTime)
        {
            PartitionKey = recordedBy;
            RowKey = recordTime?.Ticks.ToString();
        }
    }
}
