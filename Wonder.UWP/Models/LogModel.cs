using System;

namespace Wonder.UWP.Models
{
    public class LogModel
    {
        public LogModel(string message)
        {
            Time = DateTime.Now;
            Message = message;
        }

        public DateTime Time { get; }
        public string Message { get; }
    }
}
