using Prism.Windows.Navigation;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Wonder.UWP.Logger;

namespace Wonder.UWP.ViewModels
{
    public class LESyncWriteLoggerViewModel : LEBaseLoggerViewModel
    {
        #region 属性
        private DateTime mStartedTime;
        public DateTime StartedTime
        {
            get { return mStartedTime; }
            set { SetProperty(ref mStartedTime, value); }
        }

        private DateTime mStoppedTime;
        public DateTime StoppedTime
        {
            get { return mStoppedTime; }
            set { SetProperty(ref mStoppedTime, value); }
        }

        private int mSucceedCount;
        public int SucceedCount
        {
            get { return mSucceedCount; }
            set { SetProperty(ref mSucceedCount, value); }
        }

        private long mSucceedLength;
        public long SucceedLength
        {
            get { return mSucceedLength; }
            set { SetProperty(ref mSucceedLength, value); }
        }

        private int mFailedCount;
        public int FailedCount
        {
            get { return mFailedCount; }
            set { SetProperty(ref mFailedCount, value); }
        }

        private long mFailedLength;
        public long FailedLength
        {
            get { return mFailedLength; }
            set { SetProperty(ref mFailedLength, value); }
        }

        private int mSpeed;
        public int Speed
        {
            get { return mSpeed; }
            set { SetProperty(ref mSpeed, value); }
        }
        #endregion

        #region 构造
        public LESyncWriteLoggerViewModel(INavigationService navigationService, string mac)
            : base(navigationService, $"{mac}-同步写入")
        {

        }
        #endregion

        #region 方法
        public async Task LogStartedAsync()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => HandleStarted());
            await LogAsync();
        }

        private void HandleStarted()
        {
            StartedTime = DateTime.Now;
            var message = "同步写入开始";
            Add(message);
        }

        public async Task LogWriteAsync(byte[] send, byte[] received)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => HandleWrite(send, received));
            await LogAsync();
            await CensusAsync();
        }

        protected virtual void HandleWrite(byte[] write, byte[] notify)
        {
            string message;
            var expected = write != null ? write.Length : 0;
            var actual = notify != null ? notify.Length : 0;
            var isWritten = expected == actual;
            if (isWritten)
            {
                for (int i = 0; i < write.Length; i++)
                {
                    if (write[i] != notify[i])
                    {
                        isWritten = false;
                        break;
                    }
                }
            }
            if (isWritten)
            {
                var str = notify != null ? BitConverter.ToString(notify) : "NULL";
                message = $"校验成功：{str}";
                SucceedCount++;
                SucceedLength += expected;
            }
            else
            {
                var str = notify != null ? BitConverter.ToString(notify) : "NULL";
                message = $"校验失败：{str}";
                FailedCount++;
                FailedLength += expected;
            }
            Speed = (int)(SucceedLength / (DateTime.Now - StartedTime).TotalSeconds) * 2;
            Add(message);
        }

        public async Task LogStoppedAsync()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => HandleStopped());
            await LogAsync();
        }

        protected virtual void HandleStopped()
        {
            StoppedTime = DateTime.Now;
            var message = "同步写入结束";
            Add(message);
        }

        private async Task CensusAsync()
        {
            var message = $"\r\n" +
                          $"起止时间：{StartedTime} - {StoppedTime}\r\n" +
                          $"校验成功：{SucceedCount}包 {SucceedLength}字节\r\n" +
                          $"校验失败：{FailedCount}包 {FailedLength}字节\r\n" +
                          $"平均速度：{Speed}字节/秒";
            await CensusLogger.LogAsync(message, LogMode.Newer);
        }
        #endregion
    }
}
