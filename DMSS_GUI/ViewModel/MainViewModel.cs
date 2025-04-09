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
using System.Configuration;
using System.Collections.ObjectModel;
using System.Windows;
using System.IO.Ports;
using DMSS_GUI.Interface;
using LiveCharts.Wpf;
using LiveCharts;
using System.Security.Cryptography.Xml;

namespace DMSS_GUI.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private ReceiverModel _receiverA;
        private ReceiverModel _receiverB;
        private Brush _receiverAColor = Brushes.LightGray;
        private Brush _receiverBColor = Brushes.LightGray;
        private int _diff;
        private string _systemStatus;
        private Brush _statusColor;
        public enum fMode { Auto, Manual }
        private fMode _fireMode = fMode.Auto;
        private string _selectedTarget;
        private bool _isFire;
        
        public readonly IDialogService _dialogService;
        public SerialCommunication Serial { get; private set; }

        public SeriesCollection SeriesCollection { get; set; }
        public ObservableCollection<string> Labels { get; set; }

        private int _tick = 0;
        private const int MaxPoints = 15;

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
        public Brush ReceiverAColor
        {
            get => _receiverAColor;
            set { _receiverAColor = value; OnPropertyChanged(nameof(ReceiverAColor)); }
        }

        public Brush ReceiverBColor
        {
            get => _receiverBColor;
            set { _receiverBColor = value; OnPropertyChanged(nameof(ReceiverBColor)); }
        }

        public int Diff
        {
            get => _diff;
            set { _diff = value; OnPropertyChanged("diff"); }
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

        public fMode FireMode
        {
            get => _fireMode;
            set { _fireMode = value; OnPropertyChanged(nameof(FireMode)); }
        }

        public string SelectedTarget
        {
            get => _selectedTarget;
            set { _selectedTarget = value; OnPropertyChanged(nameof(SelectedTarget)); }
        }

        public bool IsFire
        {
            get => _isFire;
            set { _isFire = value; OnPropertyChanged(nameof(IsFire)); }
        }

        public ICommand FireCommand { get; }
        public ObservableCollection<string> LogMessages { get; } = new();
        

        public ICommand ModeChangedCommand { get; }

        public MainViewModel(IDialogService dialogService)
        {
            ReceiverA = new ReceiverModel { SignalStrength = 0 };
            ReceiverB = new ReceiverModel { SignalStrength = 0 };
            SystemStatus = "발사 준비";
            StatusColor = Brushes.Blue;

            _dialogService = dialogService;
            Serial = new SerialCommunication(this);

            TryAutoConnect();

            SeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "ReceiverA",  // 이름
                    Values = new ChartValues<int>(),  // 값의 자료형
                    PointGeometry = DefaultGeometries.Circle, //점의 모양
                    Stroke = System.Windows.Media.Brushes.Blue,
                    Fill = System.Windows.Media.Brushes.Transparent
                },
                new LineSeries
                {
                    Title = "ReceiverB",  // 이름
                    Values = new ChartValues<int>(),  // 값의 자료형
                    PointGeometry = DefaultGeometries.Circle, //점의 모양
                    Stroke = System.Windows.Media.Brushes.Red,
                    Fill = System.Windows.Media.Brushes.Transparent
                }
            };

            Labels = new ObservableCollection<string>();

            FireCommand = new RelayCommand(async (_) => await FireAsync());
        }

        public void AddSignalData(int valueA, int valueB)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                SeriesCollection[0].Values.Add(valueA);
                SeriesCollection[1].Values.Add(valueB);
                Labels.Add($"{_tick++}s");

                if (SeriesCollection[0].Values.Count > MaxPoints)
                {
                    SeriesCollection[0].Values.RemoveAt(0);
                    SeriesCollection[1].Values.RemoveAt(0);
                    Labels.RemoveAt(0);
                }

                LogMessages.Add($"[{DateTime.Now:HH:mm:ss}] A: {valueA}, B: {valueB}");
                if (LogMessages.Count > 100)
                    LogMessages.RemoveAt(0);

                OnPropertyChanged(nameof(SeriesCollection));
                OnPropertyChanged(nameof(Labels));
                OnPropertyChanged(nameof(LogMessages));
            });
        }

        private void TryAutoConnect()
        {
            string defaultPort = "COM3";

            bool connected = Serial.ConnectToPort(defaultPort);
            if (connected)
            {
                AddLog($"[System] {defaultPort} 자동 연결 성공");
                return;
            }

            AddLog($"[System] {defaultPort} 연결 실패 → 포트 선택 요청");

            string[] ports = SerialPort.GetPortNames();
            if (ports.Length == 0)
            {
                _dialogService.ShowMessage("사용 가능한 포트가 없습니다.");
                return;
            }

            string? selectedPort = _dialogService.SelectSerialPort(ports);
            if (string.IsNullOrEmpty(selectedPort))
            {
                _dialogService.ShowMessage("포트 선택이 취소되었습니다.");
                return;
            }

            connected = Serial.ConnectToPort(selectedPort);
            if (connected)
            {
                AddLog($"[System] {selectedPort} 연결 성공");
            }
            else
            {
                _dialogService.ShowMessage($"{selectedPort} 포트 연결에 실패했습니다.");
            }
        }

        private async Task FireAsync()
        {
            if (IsFire) return; // 중복 방지
            IsFire = true;

            SystemStatus = "발사 진행 중...";
            StatusColor = Brushes.Orange;

            bool result = await Serial.SendFireCommandAsync(timeoutMilliseconds: 3000); // 발사 프로세스

            if (result)
            {
                SystemStatus = "발사 성공!";
                StatusColor = Brushes.Green;
            }
            else
            {
                SystemStatus = "발사 실패!";
                StatusColor = Brushes.Red;
            }

            IsFire = false;
        }

        public void UpdateReceiverColors()
        {
            if(Diff > 0)
            {
                ReceiverAColor = Brushes.LightGreen;
                ReceiverBColor = Brushes.LightGray;

            } else if(Diff < 0)
            {
                ReceiverAColor = Brushes.LightGray;
                ReceiverBColor = Brushes.LightGreen;
            } 
            else
            {
                ReceiverAColor = Brushes.LightYellow;
                ReceiverBColor = Brushes.LightYellow;
            }
        }

        public void AddLog(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                LogMessages.Add($"{DateTime.Now:HH:mm:ss} - {message}");
            });
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}