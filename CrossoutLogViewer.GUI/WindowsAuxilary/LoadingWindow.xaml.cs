using CrossoutLogView.Common;
using CrossoutLogView.GUI.Core;

using MahApps.Metro.Controls;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CrossoutLogView.GUI.WindowsAuxilary
{
    /// <summary>
    /// Interaction logic for LoadingWindow.xaml
    /// </summary>
    public partial class LoadingWindow : MetroWindow, ILogging
    {
        Timer closeTimer = new Timer { Interval = 250, AutoReset = false };
        bool forceClose = false;

        public LoadingWindow()
        {
            logger.TraceResource("WinInit");
            InitializeComponent();
            DataContext = new WindowViewModelBase();
            closeTimer.Elapsed += (s, e) => Dispatcher.Invoke(Close);
            Topmost = true;
            logger.TraceResource("WinInitD");
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (e is null || forceClose)
            {
                closeTimer.Dispose();
            }
            else
            {
                closeTimer.Start();
                e.Cancel = true;
                forceClose = true;
            }
        }

        public string Header { get => GetValue(HeaderProperty) as string; set => SetValue(HeaderProperty, value); }
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(nameof(Header), typeof(string), typeof(LoadingWindow));
        public string Message { get => GetValue(MessageProperty) as string; set => SetValue(MessageProperty, value); }
        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(nameof(Message), typeof(string), typeof(LoadingWindow));

        public double Minimum { get => (double)GetValue(MinimumProperty); set => SetValue(MinimumProperty, value); }
        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(LoadingWindow));

        public double Maximum { get => (double)GetValue(MaximumProperty); set => SetValue(MaximumProperty, value); }
        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(LoadingWindow));

        public double Value { get => (double)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(double), typeof(LoadingWindow));

        public bool IsIndeterminate { get => (bool)GetValue(IsIndeterminateProperty); set => SetValue(IsIndeterminateProperty, value); }
        public static readonly DependencyProperty IsIndeterminateProperty = DependencyProperty.Register(nameof(IsIndeterminate), typeof(bool), typeof(LoadingWindow));

        #region ILogging support
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        NLog.Logger ILogging.Logger { get; } = logger;
        #endregion
    }
}
