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
        private readonly IList<int> _rssis;

        public bool IsStressWriting { get; private set; }

        public MemoryLELoggerX()
        {
            _rssis = new List<int>();
            Logs = new ObservableCollection<Log>();
        }

        #region RSSI
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
        #endregion

        #region CENSUS
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
        #endregion

        #region STRESS WRITE
        private DateTime _startStressWriteTime;
        public DateTime StartStressWriteTime
        {
            get { return _startStressWriteTime; }
            set { SetProperty(ref _startStressWriteTime, value); }
        }

        private DateTime _stopStressWriteTime;
        public DateTime StopStressWriteTime
        {
            get { return _stopStressWriteTime; }
            set { SetProperty(ref _stopStressWriteTime, value); }
        }

        private int _stressSendCount;
        public int StressSendCount
        {
            get { return _stressSendCount; }
            set { SetProperty(ref _stressSendCount, value); }
        }

        private int _stressFailedCount;
        public int StressFailedCount
        {
            get { return _stressFailedCount; }
            set { SetProperty(ref _stressFailedCount, value); }
        }

        private int _stressReceivedCount;
        public int StressReceivedCount
        {
            get { return _stressReceivedCount; }
            set { SetProperty(ref _stressReceivedCount, value); }
        }

        private long _stressSendLength;
        public long StressSendLength
        {
            get { return _stressSendLength; }
            set { SetProperty(ref _stressSendLength, value); }
        }

        private long _stressFailedLength;
        public long StressFailedLength
        {
            get { return _stressFailedLength; }
            set { SetProperty(ref _stressFailedLength, value); }
        }

        private long _stressReceivedLength;
        public long StressReceivedLength
        {
            get { return _stressReceivedLength; }
            set { SetProperty(ref _stressReceivedLength, value); }
        }

        private long _checkFailedCount;
        public long CheckFailedCount
        {
            get { return _checkFailedCount; }
            set { SetProperty(ref _checkFailedCount, value); }
        }

        private long _checkFailedLength;
        public long CheckFailedLength
        {
            get { return _checkFailedLength; }
            set { SetProperty(ref _checkFailedLength, value); }
        }

        private int _stessWriteSpeed;
        public int StressWriteSpeed
        {
            get { return _stessWriteSpeed; }
            set { SetProperty(ref _stessWriteSpeed, value); }
        }
        #endregion

        public IList<Log> Logs { get; }

        public async void LogRSSI(int rssi)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                   CoreDispatcherPriority.Normal,
                   () => HandleRSSI(rssi));
        }

        protected virtual void HandleRSSI(int rssi)
        {
            _rssis.Add(rssi);
            CurrentRSSI = rssi;
            AverageRSSI = (int)_rssis.Average();
            MinimumRSSI = _rssis.Min();
            MaximumRSSI = _rssis.Max();
            var log = new Log($"RSSI: {rssi}");
            Logs.Insert(0, log);
        }

        public async void LogReceived(byte[] value, bool result = true)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                   CoreDispatcherPriority.Normal,
                   () => HandleReceived(value, result));
        }

        protected virtual void HandleReceived(byte[] value, bool result)
        {
            if (IsStressWriting)
            {
                if (result)
                {
                    StressReceivedCount++;
                    StressReceivedLength += value.Length;
                    var log = new Log($"校验成功：{BitConverter.ToString(value)}");
                    Logs.Insert(0, log);
                }
                else
                {
                    CheckFailedCount++;
                    CheckFailedLength += value.Length;
                    var log = new Log($"校验失败：{BitConverter.ToString(value)}");
                    Logs.Insert(0, log);
                }
                StressWriteSpeed = (int)((StressSendLength + StressReceivedLength + CheckFailedLength) / (DateTime.Now - StartStressWriteTime).TotalSeconds);
            }
            else
            {
                ReceivedCount++;
                ReceivedLength += value.Length;
                var log = new Log($"接收成功：{BitConverter.ToString(value)}");
                Logs.Insert(0, log);
            }
        }

        public async void LogSend(byte[] value, bool result)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                   CoreDispatcherPriority.Normal,
                   () => HandleSend(value, result));
        }

        protected virtual void HandleSend(byte[] value, bool result)
        {
            if (IsStressWriting)
            {
                if (result)
                {
                    StressSendCount++;
                    StressSendLength += value.Length;
                    StressWriteSpeed = (int)((StressSendLength + StressReceivedLength + CheckFailedLength) / (DateTime.Now - StartStressWriteTime).TotalSeconds);
                    var log = new Log($"发送成功：{BitConverter.ToString(value)}");
                    Logs.Insert(0, log);
                }
                else
                {
                    StressFailedCount++;
                    StressFailedLength += value.Length;
                    var log = new Log($"发送失败：{BitConverter.ToString(value)}");
                    Logs.Insert(0, log);
                }
            }
            else
            {
                if (result)
                {
                    SendCount++;
                    SendLength += value.Length;
                    var log = new Log($"发送成功：{BitConverter.ToString(value)}");
                    Logs.Insert(0, log);
                }
                else
                {
                    FailedCount++;
                    FailedLength += value.Length;
                    var log = new Log($"发送失败：{BitConverter.ToString(value)}");
                    Logs.Insert(0, log);
                }
            }
        }

        public async void LogStressWriteStarted()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                   CoreDispatcherPriority.Normal,
                   () => HandleStressWriteStarted());
        }

        protected virtual void HandleStressWriteStarted()
        {
            IsStressWriting = true;
            StartStressWriteTime = DateTime.Now;
            StopStressWriteTime = default;
            CheckFailedCount = 0;
            StressFailedCount = 0;
            StressFailedLength = 0;
            StressReceivedCount = 0;
            StressReceivedLength = 0;
            StressSendCount = 0;
            StressSendLength = 0;
            StressWriteSpeed = 0;
            var log = new Log("压力测试开始");
            Logs.Insert(0, log);
        }

        public async void LogStressWriteStopped()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                   CoreDispatcherPriority.Normal,
                   () => HandleStressWriteStopped());
        }

        protected virtual void HandleStressWriteStopped()
        {
            IsStressWriting = false;
            StopStressWriteTime = DateTime.Now;
            var log = new Log("压力测试结束");
            Logs.Insert(0, log);
        }
    }
}
