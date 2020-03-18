using Prism.Commands;
using Prism.Windows.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wonder.UWP.Models;
using Wonder.UWP.Services;

namespace Wonder.UWP.ViewModels
{
    public class MonitorViewModel : BaseViewModel
    {
        protected IMajorMonitorService MajorMonitorService { get; }
        protected IMinorMonitorService MinorMonitorService { get; }

        private bool mIsMajorMonitoring;
        public bool IsMajorMonitoring
        {
            get { return mIsMajorMonitoring; }
            set { SetProperty(ref mIsMajorMonitoring, value); }
        }

        private bool mIsMinorMonitoring;
        public bool IsMinorMonitoring
        {
            get { return mIsMinorMonitoring; }
            set { SetProperty(ref mIsMinorMonitoring, value); }
        }

        public IList<ValueModel> ValueModels { get; }

        public MonitorViewModel(INavigationService navigationService, IMajorMonitorService majorMonitorService, IMinorMonitorService minorMonitorService)
            : base(navigationService)
        {
            MajorMonitorService = majorMonitorService;
            MinorMonitorService = minorMonitorService;
            IsMajorMonitoring = majorMonitorService.IsMonitoring;
            IsMinorMonitoring = minorMonitorService.IsMonitoring;
            ValueModels = new ObservableCollection<ValueModel>();
            MajorMonitorService.StateChanged += OnMajorStateChanged;
            MajorMonitorService.ValueChanged += OnMajorValueChanged;
            MinorMonitorService.StateChanged += OnMinorStateChanged;
            MinorMonitorService.ValueChanged += OnMinorValueChanged;

            StartMajorService();
        }

        private async void StartMajorService()
        {
            await MajorMonitorService.StartAsync();
        }

        private async void OnMajorValueChanged(object sender, ValueEventArgs e)
        {
            var model = new ValueModel(0, e.Value);
            await DispatcherRunAsync(() => ValueModels.Add(model));
            await MinorMonitorService.SendAsync(e.Value);
        }

        private void OnMajorStateChanged(object sender, EventArgs e)
        {
            IsMajorMonitoring = MajorMonitorService.IsMonitoring;
        }

        private async void OnMinorValueChanged(object sender, ValueEventArgs e)
        {
            var model = new ValueModel(1, e.Value);
            await DispatcherRunAsync(() => ValueModels.Add(model));
            await MajorMonitorService.SendAsync(e.Value);
        }

        private void OnMinorStateChanged(object sender, EventArgs e)
        {
            IsMinorMonitoring = MinorMonitorService.IsMonitoring;
        }

        private DelegateCommand mStartCommand;
        public DelegateCommand StartCommand =>
            mStartCommand ?? (mStartCommand = new DelegateCommand(ExecuteStartCommand, CanExecuteStartCommand).ObservesProperty(() => IsMinorMonitoring));

        private bool CanExecuteStartCommand()
        {
            return !IsMinorMonitoring;
        }

        async void ExecuteStartCommand()
        {
            await MinorMonitorService.StartAsync();
        }

        private DelegateCommand mStopCommand;
        public DelegateCommand StopCommand =>
            mStopCommand ?? (mStopCommand = new DelegateCommand(ExecuteStopCommand, CanExecuteStopCommand).ObservesProperty(() => IsMinorMonitoring));

        private bool CanExecuteStopCommand()
        {
            return IsMinorMonitoring;
        }

        void ExecuteStopCommand()
        {
            MinorMonitorService.Stop();
        }

        private DelegateCommand mSwitchStateCommand;
        public DelegateCommand SwitchStateCommand =>
            mSwitchStateCommand ?? (mSwitchStateCommand = new DelegateCommand(ExecuteSwitchStateCommand));

        void ExecuteSwitchStateCommand()
        {
            if (StartCommand.CanExecute())
            {
                StartCommand.Execute();
            }
            else
            {
                StopCommand.Execute();
            }
        }

        private DelegateCommand mClearCommand;
        public DelegateCommand ClearCommand =>
            mClearCommand ?? (mClearCommand = new DelegateCommand(ExecuteClearCommand));

        void ExecuteClearCommand()
        {
            ValueModels.Clear();
        }
    }
}
