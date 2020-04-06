using System;

namespace RepostAspNet
{
    public class StatusException : Exception
    {
        public StatusException(int status, string detail)
        {
            Status = status;
            Detail = detail;
        }

        public int Status { get; set; }
        public string Detail { get; set; }
    }
}