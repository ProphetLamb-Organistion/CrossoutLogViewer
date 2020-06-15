using ControlzEx.Theming;

using CrossoutLogView.Common;
using CrossoutLogView.Database.Data;
using CrossoutLogView.GUI.Core;
using CrossoutLogView.GUI.Models;
using CrossoutLogView.GUI.Navigation;
using CrossoutLogView.GUI.Services;

using MahApps.Metro.Controls;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CrossoutLogView.GUI.WindowsAuxilary
{
    /// <summary>
    /// Interaction logic for NavigationWindow.xaml
    /// </summary>
    public partial class NavigationWindow : MetroWindow, ILogging
    {
        private readonly ViewModelBase viewModel;

        public NavigationWindow(ViewModelBase viewModel)
        {
            if (Settings.Current.StartupMaximized)
                WindowState = WindowState.Maximized;
            logger.TraceResource("WinInit");
            InitializeComponent();
            logger.TraceResource("WinInitD");
            logger.TraceResource("ViewModelInit");
            DataContext = new WindowViewModelBase();
            this.viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (viewModel is UserModel um) frame.Navigate(new UserPage(this, um));
            else if (viewModel is GameModel gm) frame.Navigate(new GamePage(this , gm));
            else if (viewModel is PlayerModel pm) frame.Navigate(new PlayerPage(this, pm));
            else if (viewModel is UserListModel ul) frame.Navigate(new UserListPage(this, ul));
            else throw new InvalidOperationException(App.GetSharedResource("Excp_UnupportedVM"));
            logger.TraceResource("ViewModelInitD");
        }

        public void Navigate(object content)
        {
            if (content == null) return;
            logger.Trace(App.GetLogResource("Navi_NavigateTo") + content.GetType().FullName);
            frame.Navigate(content);
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            if (frame.CanGoBack) frame.GoBack();
            e.Handled = true;
        }

        private void GoForward(object sender, RoutedEventArgs e)
        {
            if (frame.CanGoForward) frame.GoForward();
            e.Handled = true;
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (IsLoaded)
                Settings.Current.StartupMaximized = WindowState == WindowState.Maximized;
        }

        #region ILogging support
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        NLog.Logger ILogging.Logger { get; } = logger;
        #endregion
    }
}
