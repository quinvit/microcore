namespace ReportService.Interface.Models
{
    public class DailyTimeByUser
    {
        public string Description { get; set; }

        public int TotalMinutes { get; set; }

        public int DayInMonth { get; set; }

        public int MonthInYear { get; set; }

        public string Email { get; set; }
    }
}
