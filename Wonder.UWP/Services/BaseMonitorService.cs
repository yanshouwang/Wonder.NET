using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wonder.UWP.Services
{
    abstract class BaseMonitorService : IMajorMonitorService, IMinorMonitorService
    {
        public event EventHandler StateChanged;
        public event EventHandler<ValueEventArgs> ValueChanged;

        private bool _isMonitoring;
        public bool IsMonitoring
        {
            get => _isMonitoring;
            protected set
            {
                if (_isMonitoring == value)
                    return;

                _isMonitoring = value;
                StateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public async Task StartAsync()
        {
            if (IsMonitoring)
                return;
            IsMonitoring = await TryStartAsync();
        }

        public async Task SendAsync(byte[] value)
        {
            if (!IsMonitoring)
                return;
            await TrySendAsync(value);
        }

        public void Stop()
        {
            if (!IsMonitoring)
                return;
            IsMonitoring = !TryStop();
        }

        protected void RaiseValueChanged(byte[] value)
        {
            var args = new ValueEventArgs(value);
            ValueChanged?.Invoke(this, args);
        }

        protected abstract Task<bool> TryStartAsync();
        protected abstract bool TryStop();
        protected abstract Task TrySendAsync(byte[] value);
    }
}
