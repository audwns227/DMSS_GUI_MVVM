using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using DMSS_GUI.Model;
using DMSS_GUI.command;

namespace DMSS_GUI.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private ReceiverModel _receiverA;
        private ReceiverModel _receiverB;
        private string _systemStatus;
        private Brush _statusColor;
        private bool _isManualMode;
        private bool _targetASelected;
        private bool _targetBSelected;

        public ReceiverModel ReceiverA
        {
            get => _receiverA;
            set { _receiverA = value; OnPropertyChanged(nameof(ReceiverA)); }
        }

        public ReceiverModel ReceiverB
        {
            get => _receiverB;
            set { _receiverB = value; OnPropertyChanged(nameof(ReceiverB)); }
        }

        public string SystemStatus
        {
            get => _systemStatus;
            set { _systemStatus = value; OnPropertyChanged(nameof(SystemStatus)); }
        }

        public Brush StatusColor
        {
            get => _statusColor;
            set { _statusColor = value; OnPropertyChanged(nameof(StatusColor)); }
        }

        public bool IsManualMode
        {
            get => _isManualMode;
            set { _isManualMode = value; OnPropertyChanged(nameof(IsManualMode)); }
        }

        public bool TargetASelected
        {
            get => _targetASelected;
            set { _targetASelected = value; OnPropertyChanged(nameof(TargetASelected)); }
        }

        public bool TargetBSelected
        {
            get => _targetBSelected;
            set { _targetBSelected = value; OnPropertyChanged(nameof(TargetBSelected)); }
        }

        public ICommand FireCommand { get; }
        public ICommand ModeChangedCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainViewModel()
        {
            ReceiverA = new ReceiverModel { SignalStrength = 70 };
            ReceiverB = new ReceiverModel { SignalStrength = 30 };
            SystemStatus = "발사 준비";
            StatusColor = Brushes.Blue;

            FireCommand = new RelayCommand(async (_) => await FireAsync());
            ModeChangedCommand = new RelayCommand(ChangeMode);
        }

        private async Task FireAsync()
        {
            SystemStatus = "발사 진행 중...";
            StatusColor = Brushes.Orange;

            await Task.Delay(1000); // 가상의 발사 프로세스

            bool success = new Random().Next(0, 2) == 1;
            if (success)
            {
                SystemStatus = "발사 성공!";
                StatusColor = Brushes.Green;
            }
            else
            {
                SystemStatus = "발사 실패!";
                StatusColor = Brushes.Red;
            }

            ReceiverA.SignalStrength = new Random().Next(30, 100);
            ReceiverB.SignalStrength = new Random().Next(10, 80);
        }

        private void ChangeMode(object parameter)
        {
            IsManualMode = (bool)parameter;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}