namespace ReportService.Interface.Models
{
    public class DailyTimeByUser
    {
        public string Name { get; set; }

        public int TotalMinutes { get; set; }

        public int DayInMonth { get; set; }

        public string Email { get; set; }
    }
}
