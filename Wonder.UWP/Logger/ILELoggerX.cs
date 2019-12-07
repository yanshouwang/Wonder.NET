using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wonder.UWP.Logger
{
    public interface ILELoggerX : ILELogger
    {
        int CurrentRSSI { get; }
        int AverageRSSI { get; }
        int MinimumRSSI { get; }
        int MaximumRSSI { get; }
        int WriteSucceedCount { get; }
        long WriteSucceedLength { get; }
        int WriteFailedCount { get; }
        long WriteFailedLength { get; }
        int ValueChangedCount { get; }
        long ValueChangedLength { get; }
        DateTime LoopWriteStartedTime { get; }
        DateTime LoopWriteStoppedTime { get; }
        int LoopWriteSucceedCount { get; }
        long LoopWriteSucceedLength { get; }
        int LoopWriteFailedCount { get; }
        long LoopWriteFailedLength { get; }
        int LoopWriteSpeed { get; }
        DateTime SyncWriteStartedTime { get; }
        DateTime SyncWriteStoppedTime { get; }
        int SyncWriteSucceedCount { get; }
        long SyncWriteSucceedLength { get; }
        int SyncWriteFailedCount { get; }
        long SyncWriteFailedLength { get; }
        int SyncReceivedCount { get; }
        long SyncReceivedLength { get; }
        int SyncWriteSpeed { get; }
        IList<Log> Logs { get; }
    }
}
