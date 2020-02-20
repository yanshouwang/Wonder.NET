﻿using Prism.Mvvm;
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
        private int _writeSucceedCount;
        public int WriteSucceedCount
        {
            get { return _writeSucceedCount; }
            set { SetProperty(ref _writeSucceedCount, value); }
        }

        private long _writeSucceedLength;
        public long WriteSucceedLength
        {
            get { return _writeSucceedLength; }
            set { SetProperty(ref _writeSucceedLength, value); }
        }

        private int _writeFailedCount;
        public int WriteFailedCount
        {
            get { return _writeFailedCount; }
            set { SetProperty(ref _writeFailedCount, value); }
        }

        private long _writeFailedLength;
        public long WriteFailedLength
        {
            get { return _writeFailedLength; }
            set { SetProperty(ref _writeFailedLength, value); }
        }

        private int _valueChangedCount;
        public int ValueChangedCount
        {
            get { return _valueChangedCount; }
            set { SetProperty(ref _valueChangedCount, value); }
        }

        private long _valueChangedLength;
        public long ValueChangedLength
        {
            get { return _valueChangedLength; }
            set { SetProperty(ref _valueChangedLength, value); }
        }
        #endregion

        #region LOOP WRITE
        private DateTime _loopWriteStartedTime;
        public DateTime LoopWriteStartedTime
        {
            get { return _loopWriteStartedTime; }
            set { SetProperty(ref _loopWriteStartedTime, value); }
        }

        private DateTime _loopWriteStoppedTime;
        public DateTime LoopWriteStoppedTime
        {
            get { return _loopWriteStoppedTime; }
            set { SetProperty(ref _loopWriteStoppedTime, value); }
        }

        private int _loopWriteSucceedCount;
        public int LoopWriteSucceedCount
        {
            get { return _loopWriteSucceedCount; }
            set { SetProperty(ref _loopWriteSucceedCount, value); }
        }

        private long _loopWriteSucceedLength;
        public long LoopWriteSucceedLength
        {
            get { return _loopWriteSucceedLength; }
            set { SetProperty(ref _loopWriteSucceedLength, value); }
        }

        private int _loopWriteFailedCount;
        public int LoopWriteFailedCount
        {
            get { return _loopWriteFailedCount; }
            set { SetProperty(ref _loopWriteFailedCount, value); }
        }

        private long _loopWriteFailedLength;
        public long LoopWriteFailedLength
        {
            get { return _loopWriteFailedLength; }
            set { SetProperty(ref _loopWriteFailedLength, value); }
        }

        private int _loopWriteSpeed;
        public int LoopWriteSpeed
        {
            get { return _loopWriteSpeed; }
            set { SetProperty(ref _loopWriteSpeed, value); }
        }
        #endregion

        #region SYNC WRITE
        private DateTime _syncWriteStartedTime;
        public DateTime SyncWriteStartedTime
        {
            get { return _syncWriteStartedTime; }
            set { SetProperty(ref _syncWriteStartedTime, value); }
        }

        private DateTime _syncWriteStoppedTime;
        public DateTime SyncWriteStoppedTime
        {
            get { return _syncWriteStoppedTime; }
            set { SetProperty(ref _syncWriteStoppedTime, value); }
        }

        private int _syncWriteSucceedCount;
        public int SyncWriteSucceedCount
        {
            get { return _syncWriteSucceedCount; }
            set { SetProperty(ref _syncWriteSucceedCount, value); }
        }

        private long _syncWriteSucceedLength;
        public long SyncWriteSucceedLength
        {
            get { return _syncWriteSucceedLength; }
            set { SetProperty(ref _syncWriteSucceedLength, value); }
        }

        private int _syncWriteFailedCount;
        public int SyncWriteFailedCount
        {
            get { return _syncWriteFailedCount; }
            set { SetProperty(ref _syncWriteFailedCount, value); }
        }

        private long _syncReceivedLength;
        public long SyncReceivedLength
        {
            get { return _syncReceivedLength; }
            set { SetProperty(ref _syncReceivedLength, value); }
        }

        private int _syncReceivedCount;
        public int SyncReceivedCount
        {
            get { return _syncReceivedCount; }
            set { SetProperty(ref _syncReceivedCount, value); }
        }

        private long _syncWriteFailedLength;
        public long SyncWriteFailedLength
        {
            get { return _syncWriteFailedLength; }
            set { SetProperty(ref _syncWriteFailedLength, value); }
        }

        private int _syncWriteSpeed;
        public int SyncWriteSpeed
        {
            get { return _syncWriteSpeed; }
            set { SetProperty(ref _syncWriteSpeed, value); }
        }
        #endregion

        #region CONTINUOUS UPLOAD
        public DateTime ContinuousUploadStartedTime => throw new NotImplementedException();

        public DateTime ContinuousUploadStoppedTime => throw new NotImplementedException();

        public int ContinuousUploadCount => throw new NotImplementedException();

        public long ContinuousUploadLength => throw new NotImplementedException();

        public int ContinuousUploadSpeed => throw new NotImplementedException();
        #endregion

        public IList<Log> Logs { get; }

        public MemoryLELoggerX()
        {
            _rssis = new List<int>();
            Logs = new ObservableCollection<Log>();
        }

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

        public async void LogValueChanged(byte[] value)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                   CoreDispatcherPriority.Normal,
                   () => HandleValueChanged(value));
        }

        protected virtual void HandleValueChanged(byte[] value)
        {
            ValueChangedCount++;
            ValueChangedLength += value.Length;
            var log = new Log($"接收成功：{BitConverter.ToString(value)}");
            Logs.Insert(0, log);
        }

        public async void LogWrite(byte[] value, bool result)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                   CoreDispatcherPriority.Normal,
                   () => HandleWrite(value, result));
        }

        protected virtual void HandleWrite(byte[] value, bool result)
        {
            if (result)
            {
                WriteSucceedCount++;
                WriteSucceedLength += value.Length;
                var log = new Log($"发送成功：{BitConverter.ToString(value)}");
                Logs.Insert(0, log);
            }
            else
            {
                WriteFailedCount++;
                WriteFailedLength += value.Length;
                var log = new Log($"发送失败：{BitConverter.ToString(value)}");
                Logs.Insert(0, log);
            }
        }

        public async void LogLoopWriteStarted()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                   CoreDispatcherPriority.Normal,
                   () => HandleLoopWriteStarted());
        }

        protected virtual void HandleLoopWriteStarted()
        {
            LoopWriteStartedTime = DateTime.Now;
            LoopWriteStoppedTime = default;
            LoopWriteSucceedCount = 0;
            LoopWriteSucceedLength = 0;
            LoopWriteFailedCount = 0;
            LoopWriteFailedLength = 0;
            LoopWriteSpeed = 0;
            var log = new Log("循环写入开始");
            Logs.Insert(0, log);
        }

        public async void LogLoopWrite(byte[] value, bool result)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                   CoreDispatcherPriority.Normal,
                   () => HandleLoopWrite(value, result));
        }

        protected virtual void HandleLoopWrite(byte[] value, bool result)
        {
            if (result)
            {
                LoopWriteSucceedCount++;
                LoopWriteSucceedLength += value.Length;
                LoopWriteSpeed = (int)(LoopWriteSucceedLength / (DateTime.Now - LoopWriteStartedTime).TotalSeconds);
                var log = new Log($"发送成功：{BitConverter.ToString(value)}");
                Logs.Insert(0, log);
            }
            else
            {
                LoopWriteFailedCount++;
                LoopWriteFailedLength += value.Length;
                var log = new Log($"发送失败：{BitConverter.ToString(value)}");
                Logs.Insert(0, log);
            }
        }

        public async void LogLoopWriteStopped()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                   CoreDispatcherPriority.Normal,
                   () => HandleLoopWriteStopped());
        }

        protected virtual void HandleLoopWriteStopped()
        {
            LoopWriteStoppedTime = DateTime.Now;
            var log = new Log("循环写入结束");
            Logs.Insert(0, log);
        }

        public async void LogSyncWriteStarted()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                   CoreDispatcherPriority.Normal,
                   () => HandleSyncWriteStarted());
        }

        protected virtual void HandleSyncWriteStarted()
        {
            SyncWriteStartedTime = DateTime.Now;
            SyncWriteStoppedTime = default;
            SyncWriteSucceedCount = 0;
            SyncWriteSucceedLength = 0;
            SyncWriteFailedCount = 0;
            SyncWriteFailedLength = 0;
            SyncReceivedCount = 0;
            SyncReceivedLength = 0;
            SyncWriteSpeed = 0;
            var log = new Log("同步写入开始");
            Logs.Insert(0, log);
        }

        public async void LogSyncWrite(byte[] send, byte[] received)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                   CoreDispatcherPriority.Normal,
                   () => HandleSyncWrite(send, received));
        }

        protected virtual void HandleSyncWrite(byte[] send, byte[] received)
        {
            SyncReceivedCount++;
            SyncReceivedLength += received.Length;
            var isWritten = send.Length == received.Length;
            if (isWritten)
            {
                for (int i = 0; i < send.Length; i++)
                {
                    if (send[i] != received[i])
                    {
                        isWritten = false;
                        break;
                    }
                }
            }
            if (isWritten)
            {
                SyncWriteSucceedCount++;
                SyncWriteSucceedLength += send.Length;
                SyncWriteSpeed = (int)((SyncWriteSucceedLength + SyncWriteFailedLength + SyncReceivedLength) / (DateTime.Now - SyncWriteStartedTime).TotalSeconds);
                var log = new Log($"校验成功：{BitConverter.ToString(received)}");
                Logs.Insert(0, log);
            }
            else
            {
                SyncWriteFailedCount++;
                SyncWriteFailedLength += send.Length;
                SyncWriteSpeed = (int)((SyncWriteSucceedLength + SyncWriteFailedLength + SyncReceivedLength) / (DateTime.Now - SyncWriteStartedTime).TotalSeconds);
                var log = new Log($"校验失败：{BitConverter.ToString(received)}");
                Logs.Insert(0, log);
            }
        }

        public async void LogSyncWriteStopped()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                   CoreDispatcherPriority.Normal,
                   () => HandleSyncWriteStopped());
        }

        protected virtual void HandleSyncWriteStopped()
        {
            SyncWriteStoppedTime = DateTime.Now;
            var log = new Log("同步写入结束");
            Logs.Insert(0, log);
        }

        public void LogContinuousUploadStarted()
        {
            throw new NotImplementedException();
        }

        public void LogContinuousUpload(byte[] value)
        {
            throw new NotImplementedException();
        }

        public void LogContinuousUploadStopped()
        {
            throw new NotImplementedException();
        }
    }
}
