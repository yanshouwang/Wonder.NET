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

        private bool _isMajorMonitoring;
        public bool IsMajorMonitoring
        {
            get { return _isMajorMonitoring; }
            set { SetProperty(ref _isMajorMonitoring, value); }
        }

        private bool _isMinorMonitoring;
        public bool IsMinorMonitoring
        {
            get { return _isMinorMonitoring; }
            set { SetProperty(ref _isMinorMonitoring, value); }
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

        private DelegateCommand _startCommand;
        public DelegateCommand StartCommand =>
            _startCommand ?? (_startCommand = new DelegateCommand(ExecuteStartCommand, CanExecuteStartCommand).ObservesProperty(() => IsMinorMonitoring));

        private bool CanExecuteStartCommand()
        {
            return !IsMinorMonitoring;
        }

        async void ExecuteStartCommand()
        {
            await MinorMonitorService.StartAsync();
        }

        private DelegateCommand _stopCommand;
        public DelegateCommand StopCommand =>
            _stopCommand ?? (_stopCommand = new DelegateCommand(ExecuteStopCommand, CanExecuteStopCommand).ObservesProperty(() => IsMinorMonitoring));

        private bool CanExecuteStopCommand()
        {
            return IsMinorMonitoring;
        }

        void ExecuteStopCommand()
        {
            MinorMonitorService.Stop();
        }

        private DelegateCommand _switchStateCommand;
        public DelegateCommand SwitchStateCommand =>
            _switchStateCommand ?? (_switchStateCommand = new DelegateCommand(ExecuteSwitchStateCommand));

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

        private DelegateCommand _clearCommand;
        public DelegateCommand ClearCommand =>
            _clearCommand ?? (_clearCommand = new DelegateCommand(ExecuteClearCommand));

        void ExecuteClearCommand()
        {
            ValueModels.Clear();
        }
    }
}
