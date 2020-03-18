using Prism.Windows.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Wonder.UWP.Logger;

namespace Wonder.UWP.ViewModels
{
    public class LELoggerViewModel : BaseViewModel
    {
        #region 字段
        private readonly ILogger mMessageLogger;
        private readonly ILogger mCensusLogger;
        private readonly IList<int> mRSSIs;
        #endregion

        #region 属性
        public IList<MessageViewModel> Messages { get; }

        #region RSSI
        private int mCurrentRSSI;
        public int CurrentRSSI
        {
            get { return mCurrentRSSI; }
            set { SetProperty(ref mCurrentRSSI, value); }
        }

        private int mAverageRSSI;
        public int AverageRSSI
        {
            get { return mAverageRSSI; }
            set { SetProperty(ref mAverageRSSI, value); }
        }

        private int mMinimumRSSI;
        public int MinimumRSSI
        {
            get { return mMinimumRSSI; }
            set { SetProperty(ref mMinimumRSSI, value); }
        }

        private int mMaximumRSSI;
        public int MaximumRSSI
        {
            get { return mMaximumRSSI; }
            set { SetProperty(ref mMaximumRSSI, value); }
        }
        #endregion

        #region CENSUS
        private int mWriteSucceedCount;
        public int WriteSucceedCount
        {
            get { return mWriteSucceedCount; }
            set { SetProperty(ref mWriteSucceedCount, value); }
        }

        private long mWriteSucceedLength;
        public long WriteSucceedLength
        {
            get { return mWriteSucceedLength; }
            set { SetProperty(ref mWriteSucceedLength, value); }
        }

        private int mWriteFailedCount;
        public int WriteFailedCount
        {
            get { return mWriteFailedCount; }
            set { SetProperty(ref mWriteFailedCount, value); }
        }

        private long mWriteFailedLength;
        public long WriteFailedLength
        {
            get { return mWriteFailedLength; }
            set { SetProperty(ref mWriteFailedLength, value); }
        }

        private int mValueChangedCount;
        public int ValueChangedCount
        {
            get { return mValueChangedCount; }
            set { SetProperty(ref mValueChangedCount, value); }
        }

        private long mValueChangedLength;
        public long ValueChangedLength
        {
            get { return mValueChangedLength; }
            set { SetProperty(ref mValueChangedLength, value); }
        }
        #endregion

        #region LOOP WRITE
        private DateTime mLoopWriteStartedTime;
        public DateTime LoopWriteStartedTime
        {
            get { return mLoopWriteStartedTime; }
            set { SetProperty(ref mLoopWriteStartedTime, value); }
        }

        private DateTime mLoopWriteStoppedTime;
        public DateTime LoopWriteStoppedTime
        {
            get { return mLoopWriteStoppedTime; }
            set { SetProperty(ref mLoopWriteStoppedTime, value); }
        }

        private int mLoopWriteSucceedCount;
        public int LoopWriteSucceedCount
        {
            get { return mLoopWriteSucceedCount; }
            set { SetProperty(ref mLoopWriteSucceedCount, value); }
        }

        private long mLoopWriteSucceedLength;
        public long LoopWriteSucceedLength
        {
            get { return mLoopWriteSucceedLength; }
            set { SetProperty(ref mLoopWriteSucceedLength, value); }
        }

        private int mLoopWriteFailedCount;
        public int LoopWriteFailedCount
        {
            get { return mLoopWriteFailedCount; }
            set { SetProperty(ref mLoopWriteFailedCount, value); }
        }

        private long mLoopWriteFailedLength;
        public long LoopWriteFailedLength
        {
            get { return mLoopWriteFailedLength; }
            set { SetProperty(ref mLoopWriteFailedLength, value); }
        }

        private int mLoopWriteSpeed;
        public int LoopWriteSpeed
        {
            get { return mLoopWriteSpeed; }
            set { SetProperty(ref mLoopWriteSpeed, value); }
        }
        #endregion

        #region SYNC WRITE
        private DateTime mSyncWriteStartedTime;
        public DateTime SyncWriteStartedTime
        {
            get { return mSyncWriteStartedTime; }
            set { SetProperty(ref mSyncWriteStartedTime, value); }
        }

        private DateTime mSyncWriteStoppedTime;
        public DateTime SyncWriteStoppedTime
        {
            get { return mSyncWriteStoppedTime; }
            set { SetProperty(ref mSyncWriteStoppedTime, value); }
        }

        private int mSyncWriteSucceedCount;
        public int SyncWriteSucceedCount
        {
            get { return mSyncWriteSucceedCount; }
            set { SetProperty(ref mSyncWriteSucceedCount, value); }
        }

        private long mSyncWriteSucceedLength;
        public long SyncWriteSucceedLength
        {
            get { return mSyncWriteSucceedLength; }
            set { SetProperty(ref mSyncWriteSucceedLength, value); }
        }

        private int mSyncWriteFailedCount;
        public int SyncWriteFailedCount
        {
            get { return mSyncWriteFailedCount; }
            set { SetProperty(ref mSyncWriteFailedCount, value); }
        }

        private long mSyncReceivedLength;
        public long SyncReceivedLength
        {
            get { return mSyncReceivedLength; }
            set { SetProperty(ref mSyncReceivedLength, value); }
        }

        private int mSyncReceivedCount;
        public int SyncReceivedCount
        {
            get { return mSyncReceivedCount; }
            set { SetProperty(ref mSyncReceivedCount, value); }
        }

        private long mSyncWriteFailedLength;
        public long SyncWriteFailedLength
        {
            get { return mSyncWriteFailedLength; }
            set { SetProperty(ref mSyncWriteFailedLength, value); }
        }

        private int mSyncWriteSpeed;
        public int SyncWriteSpeed
        {
            get { return mSyncWriteSpeed; }
            set { SetProperty(ref mSyncWriteSpeed, value); }
        }
        #endregion
        #endregion

        #region 构造
        public LELoggerViewModel(INavigationService navigationService, string mac)
            : base(navigationService)
        {
            if (string.IsNullOrWhiteSpace(mac))
            {
                throw new ArgumentException("日志文件名称不能为空", nameof(mac));
            }

            var time = DateTime.Now.ToFileTime();
            var fileName1 = $"{mac}-{time}-日志.txt";
            var fileName2 = $"{mac}-{time}-统计.txt";
            mMessageLogger = new StorageLogger(fileName1);
            mCensusLogger = new StorageLogger(fileName2);
            mRSSIs = new List<int>();
            Messages = new ObservableCollection<MessageViewModel>();
        }
        #endregion

        #region 方法
        public async void LogRSSI(int rssi)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                   CoreDispatcherPriority.Normal,
                   () => HandleRSSI(rssi));
        }

        private void HandleRSSI(int rssi)
        {
            mRSSIs.Add(rssi);
            CurrentRSSI = rssi;
            AverageRSSI = (int)mRSSIs.Average();
            MinimumRSSI = mRSSIs.Min();
            MaximumRSSI = mRSSIs.Max();
            var log = new MessageViewModel($"RSSI: {rssi}");
            Messages.Insert(0, log);
            LogMessage();
            LogCensus();
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
            var log = new MessageViewModel($"接收成功：{BitConverter.ToString(value)}");
            Messages.Insert(0, log);
            LogMessage();
            LogCensus();
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
                var log = new MessageViewModel($"发送成功：{BitConverter.ToString(value)}");
                Messages.Insert(0, log);
            }
            else
            {
                WriteFailedCount++;
                WriteFailedLength += value.Length;
                var log = new MessageViewModel($"发送失败：{BitConverter.ToString(value)}");
                Messages.Insert(0, log);
            }
            LogMessage();
            LogCensus();
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
            var log = new MessageViewModel("循环写入开始");
            Messages.Insert(0, log);
            LogMessage();
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
                var log = new MessageViewModel($"发送成功：{BitConverter.ToString(value)}");
                Messages.Insert(0, log);
            }
            else
            {
                LoopWriteFailedCount++;
                LoopWriteFailedLength += value.Length;
                var log = new MessageViewModel($"发送失败：{BitConverter.ToString(value)}");
                Messages.Insert(0, log);
            }
            LogMessage();
            LogCensus();
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
            var log = new MessageViewModel("循环写入结束");
            Messages.Insert(0, log);
            LogMessage();
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
            var log = new MessageViewModel("同步写入开始");
            Messages.Insert(0, log);
            LogMessage();
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
                var log = new MessageViewModel($"校验成功：{BitConverter.ToString(received)}");
                Messages.Insert(0, log);
            }
            else
            {
                SyncWriteFailedCount++;
                SyncWriteFailedLength += send.Length;
                SyncWriteSpeed = (int)((SyncWriteSucceedLength + SyncWriteFailedLength + SyncReceivedLength) / (DateTime.Now - SyncWriteStartedTime).TotalSeconds);
                var log = new MessageViewModel($"校验失败：{BitConverter.ToString(received)}");
                Messages.Insert(0, log);
            }
            LogMessage();
            LogCensus();
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
            var log = new MessageViewModel("同步写入结束");
            Messages.Insert(0, log);
            LogMessage();
        }

        private async void LogMessage()
        {
            var message = Messages[0].Message;
            await mMessageLogger.LogAsync(message);
        }

        private async void LogCensus()
        {
            var census = $"\r\n***信号强度***\r\n" +
                         $"平均值->{AverageRSSI} 最大值->{MaximumRSSI} 最小值->{MinimumRSSI}\r\n" +
                         $"***统计***\r\n" +
                         $"发送成功：{WriteSucceedCount}包 {WriteSucceedLength}字节\r\n" +
                         $"发送失败：{WriteFailedCount}包 {WriteFailedLength}字节\r\n" +
                         $"接收成功：{ValueChangedCount}包 {ValueChangedLength}字节\r\n" +
                         $"***循环发送***\r\n" +
                         $"起止时间：{LoopWriteStartedTime} - {LoopWriteStoppedTime}\r\n" +
                         $"发送成功：{LoopWriteSucceedCount}包 {LoopWriteSucceedLength}字节\r\n" +
                         $"发送失败：{LoopWriteFailedCount}包 {LoopWriteFailedLength}字节\r\n" +
                         $"平均速度：{LoopWriteSpeed}字节/秒\r\n" +
                         $"***同步测试***\r\n" +
                         $"起止时间：{SyncWriteStartedTime} - {SyncWriteStoppedTime}\r\n" +
                         $"校验成功：{SyncWriteSucceedCount}包 {SyncWriteSucceedLength}字节\r\n" +
                         $"校验失败：{SyncWriteFailedCount}包 {SyncWriteFailedLength}字节\r\n" +
                         $"平均速度：{SyncWriteSpeed}字节/秒";
            await mCensusLogger.LogAsync(census, LogMode.Newer);
        }
        #endregion
    }
}
