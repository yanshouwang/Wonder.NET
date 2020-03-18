using Prism.Windows.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Wonder.UWP.Logger;

namespace Wonder.UWP.ViewModels
{
    public class LERSSILoggerViewModel : LEBaseLoggerViewModel
    {
        #region 字段
        private readonly IList<int> mValues;
        #endregion

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

        private int mCurrent;
        public int Current
        {
            get { return mCurrent; }
            set { SetProperty(ref mCurrent, value); }
        }

        private int mAverage;
        public int Average
        {
            get { return mAverage; }
            set { SetProperty(ref mAverage, value); }
        }

        private int mMinimum;
        public int Minimum
        {
            get { return mMinimum; }
            set { SetProperty(ref mMinimum, value); }
        }

        private int mMaximum;
        public int Maximum
        {
            get { return mMaximum; }
            set { SetProperty(ref mMaximum, value); }
        }
        #endregion

        #region 构造
        public LERSSILoggerViewModel(INavigationService navigationService, string mac)
            : base(navigationService, $"{mac}-信号强度")
        {
            mValues = new List<int>();
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

        public async Task LogValueAsync(int value)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => HandleValue(value));
            await LogAsync();
            await CensusAsync();
        }

        private void HandleValue(int value)
        {
            mValues.Add(value);
            Current = value;
            Average = (int)mValues.Average();
            Minimum = mValues.Min();
            Maximum = mValues.Max();
            var message = $"信号强度: {value}";
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
                          $"平均值->{Average} 最大值->{Maximum} 最小值->{Minimum}";
            await CensusLogger.LogAsync(message, LogMode.Newer);
        }
        #endregion
    }
}
