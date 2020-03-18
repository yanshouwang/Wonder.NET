using Prism.Windows.Navigation;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Wonder.UWP.Logger;

namespace Wonder.UWP.ViewModels
{
    public class LEContinuousNotifyLoggerViewModel : LEBaseLoggerViewModel
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

        private int mNotifyCount;
        public int NotifyCount
        {
            get { return mNotifyCount; }
            set { SetProperty(ref mNotifyCount, value); }
        }

        private long mNotifyLength;
        public long NotifyLength
        {
            get { return mNotifyLength; }
            set { SetProperty(ref mNotifyLength, value); }
        }

        private int mSpeed;
        public int Speed
        {
            get { return mSpeed; }
            set { SetProperty(ref mSpeed, value); }
        }
        #endregion

        #region 构造
        public LEContinuousNotifyLoggerViewModel(INavigationService navigationService, string mac)
            : base(navigationService, $"{mac}-连续上传")
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
            var message = "开始接收";
            Add(message);
        }

        public async Task LogNotifyAsync(byte[] value)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => HandleNotify(value));
            await LogAsync();
            await CensusAsync();
        }

        private void HandleNotify(byte[] value)
        {
            NotifyCount++;
            NotifyLength += value.Length;
            Speed = (int)(NotifyLength / (DateTime.Now - StartedTime).TotalSeconds);
            string message = $"接收成功：{BitConverter.ToString(value)}";
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
            var message = "停止接收";
            Add(message);
        }

        private async Task CensusAsync()
        {
            var message = $"\r\n" +
                          $"起止时间：{StartedTime} - {StoppedTime}\r\n" +
                          $"接收成功：{NotifyCount}包 {NotifyLength}字节\r\n" +
                          $"平均速度：{Speed}字节/秒";
            await CensusLogger.LogAsync(message, LogMode.Newer);
        }
        #endregion
    }
}
