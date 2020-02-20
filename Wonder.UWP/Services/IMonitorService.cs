using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wonder.UWP.Services
{
    public interface IMonitorService
    {
        event EventHandler StateChanged;
        event EventHandler<ValueEventArgs> ValueChanged;

        bool IsMonitoring { get; }

        Task StartAsync();
        void Stop();

        Task SendAsync(byte[] value);
    }
}
