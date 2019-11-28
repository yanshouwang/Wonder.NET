using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace Wonder.UWP.Logger
{
    public class MemoryLELoggerX : BindableBase, ILELoggerX
    {
        private readonly ILELogger _logger;
        private readonly IList<int> _rssis;

        public MemoryLELoggerX(string mac)
        {
            _logger = new FileLELogger(mac);
            _rssis = new List<int>();
            Logs = new ObservableCollection<Log>();
        }

        private int _currentRSSI;
        public int CurrentRSSI
        {
            get { return _currentRSSI; }
            set { SetProperty(ref _currentRSSI, value); }
        }

        private int _averageRSSI;
        public int AverageRSSI
        {
            get { return _averageRSSI; }
            set { SetProperty(ref _averageRSSI, value); }
        }

        private int _minimumRSSI;
        public int MinimumRSSI
        {
            get { return _minimumRSSI; }
            set { SetProperty(ref _minimumRSSI, value); }
        }

        private int _maximumRSSI;
        public int MaximumRSSI
        {
            get { return _maximumRSSI; }
            set { SetProperty(ref _maximumRSSI, value); }
        }

        private int _sendCount;
        public int SendCount
        {
            get { return _sendCount; }
            set { SetProperty(ref _sendCount, value); }
        }

        private int _failedCount;
        public int FailedCount
        {
            get { return _failedCount; }
            set { SetProperty(ref _failedCount, value); }
        }

        private int _receivedCount;
        public int ReceivedCount
        {
            get { return _receivedCount; }
            set { SetProperty(ref _receivedCount, value); }
        }

        private long _sendLength;
        public long SendLength
        {
            get { return _sendLength; }
            set { SetProperty(ref _sendLength, value); }
        }

        private long _failedLength;
        public long FailedLength
        {
            get { return _failedLength; }
            set { SetProperty(ref _failedLength, value); }
        }

        private long _receivedLength;
        public long ReceivedLength
        {
            get { return _receivedLength; }
            set { SetProperty(ref _receivedLength, value); }
        }

        public IList<Log> Logs { get; }

        public async void LogRSSI(int rssi)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                   CoreDispatcherPriority.Normal,
                   () => HandleRSSI(rssi));
            _logger.LogRSSI(rssi);
        }

        private void HandleRSSI(int rssi)
        {
            _rssis.Add(rssi);
            CurrentRSSI = rssi;
            AverageRSSI = (int)_rssis.Average();
            MinimumRSSI = _rssis.Min();
            MaximumRSSI = _rssis.Max();
            var log = new Log(DateTime.Now, $"RSSI: {rssi}");
            Logs.Insert(0, log);
        }

        public async void LogReceived(byte[] value)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                   CoreDispatcherPriority.Normal,
                   () => HandleReceived(value));
            _logger.LogReceived(value);
        }

        private void HandleReceived(byte[] value)
        {
            ReceivedCount++;
            ReceivedLength += value.Length;
            var log = new Log(DateTime.Now, $"接收：{BitConverter.ToString(value)}");
            Logs.Insert(0, log);
        }

        public async void LogSend(byte[] value, bool result)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                   CoreDispatcherPriority.Normal,
                   () => HandleSend(value, result));
            _logger.LogSend(value, result);
        }

        private void HandleSend(byte[] value, bool result)
        {
            if (result)
            {
                SendCount++;
                SendLength += value.Length;
                var log = new Log(DateTime.Now, $"发送成功：{BitConverter.ToString(value)}");
                Logs.Insert(0, log);
            }
            else
            {
                FailedCount++;
                FailedLength += value.Length;
                var log = new Log(DateTime.Now, $"发送失败：{BitConverter.ToString(value)}");
                Logs.Insert(0, log);
            }
        }
    }
}
