using Prism.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wonder.UWP.Logger
{
    public interface ILELogger
    {
        bool IsStressWriting { get; }
        void LogRSSI(int rssi);
        void LogSend(byte[] value, bool result);
        void LogReceived(byte[] value, bool result = true);
        void LogStressWriteStarted();
        void LogStressWriteStopped();
    }
}
