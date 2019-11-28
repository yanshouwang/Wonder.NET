using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wonder.UWP.Logger
{
    public class Log
    {
        public Log(DateTime time, string message)
        {
            Time = time;
            Message = message;
        }

        public DateTime Time { get; }
        public string Message { get; }
    }
}
