using Prism.Windows.Navigation;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Wonder.UWP.Logger;

namespace Wonder.UWP.ViewModels
{
    public class LELoopWriteLoggerViewModel : LEBaseLoggerViewModel
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
        public LELoopWriteLoggerViewModel(INavigationService navigationService, string mac)
            : base(navigationService, $"{mac}-循环写入")
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
            var message = "循环写入开始";
            Add(message);
        }

        public async Task LogWriteAsync(byte[] value, bool result)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => HandleWrite(value, result));
            await LogAsync();
            await CensusAsync();
        }

        private void HandleWrite(byte[] value, bool result)
        {
            string message;
            if (result)
            {
                message = $"发送成功：{BitConverter.ToString(value)}";
                SucceedCount++;
                SucceedLength += value.Length;
            }
            else
            {
                message = $"发送失败：{BitConverter.ToString(value)}";
                FailedCount++;
                FailedLength += value.Length;
            }
            Speed = (int)(SucceedLength / (DateTime.Now - StartedTime).TotalSeconds);
            Add(message);
        }

        public async Task LogStoppedAsync()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => HandleStopped());
            await LogAsync();
            await CensusAsync();
        }

        private void HandleStopped()
        {
            StoppedTime = DateTime.Now;
            var message = "循环写入结束";
            Add(message);
        }

        private async Task CensusAsync()
        {
            var message = $"\r\n" +
                          $"起止时间：{StartedTime} - {StoppedTime}\r\n" +
                          $"发送成功：{SucceedCount}包 {SucceedLength}字节\r\n" +
                          $"发送失败：{FailedCount}包 {FailedLength}字节\r\n" +
                          $"平均速度：{Speed}字节/秒";
            await CensusLogger.LogAsync(message, LogMode.Newer);
        }
        #endregion
    }
}
