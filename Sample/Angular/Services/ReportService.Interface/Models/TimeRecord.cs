using System;

namespace ReportService.Interface.Models
{
    public class TimeRecord
    {
        public int TotalMinutes { get; set; }

        public string Description { get; set; }

        public string RecordedBy { get; set; }

        public DateTime? RecordedTime { get; set; }

        public string ModifiedBy { get; set; }

        public DateTime? ModifiedTime { get; set; }
    }
}
