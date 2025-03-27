﻿using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DMSS_GUI.ViewModel;

namespace DMSS_GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;
        private SerialDataReceiver _receiver;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel; // ViewModel을 DataContext로 설정

            _receiver = new SerialDataReceiver(_viewModel);
        }

        protected override void OnClosed(EventArgs e)
        {
            _receiver.Close();
            base.OnClosed(e);
        }
    }
}