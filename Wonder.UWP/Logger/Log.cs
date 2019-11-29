using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wonder.UWP.Logger
{
    public class Log
    {
        public Log(string message)
        {
            Time = DateTime.Now;
            Message = message;
        }

        public DateTime Time { get; }
        public string Message { get; }
    }
}
