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
        int SendCount { get; }
        int FailedCount { get; }
        int ReceivedCount { get; }
        long SendLength { get; }
        long FailedLength { get; }
        long ReceivedLength { get; }
        IList<Log> Logs { get; }
    }
}
