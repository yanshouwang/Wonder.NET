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
        void LogRSSI(int rssi);
        void LogWrite(byte[] value, bool result);
        void LogValueChanged(byte[] value);
        void LogLoopWriteStarted();
        void LogLoopWrite(byte[] value, bool result);
        void LogLoopWriteStopped();
        void LogSyncWriteStarted();
        void LogSyncWrite(byte[] send, byte[] received);
        void LogSyncWriteStopped();
    }
}
