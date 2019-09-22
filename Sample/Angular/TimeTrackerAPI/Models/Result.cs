namespace TimeTrackerAPI.Models
{
    public class Result
    {
        public int Code { get; set; }

        public string Message { get; set; }

        public bool IsSuccess
        {
            get
            {
                return Code == 0;
            }
        }

        public Result(int code, string message)
        {
            Code = code;
            Message = message;
        }
    }
}
