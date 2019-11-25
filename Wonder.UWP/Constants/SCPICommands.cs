using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wonder.UWP.Constants
{
    public static class SCPICommands
    {
        public const string CODE = "CODE?";
        public const string DEVICEINFO_ECHO = "DATA:DEViceinfo:ECHO?";
        public const string LIVEUPLOAD_ECHO = "DATA:LIVe:UPLoad:ECHO";
        public const string LIVE_ECHO = "DATA:LIVe:ECHO?";
        public const string TEMPTARGET_ECHO = "SOURce:TEMPerature:TARGet:ECHO";
    }
}
