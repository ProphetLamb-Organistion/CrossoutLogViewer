using ControlzEx.Theming;

using CrossoutLogView.Database.Data;
using CrossoutLogView.GUI.Core;
using CrossoutLogView.GUI.Models;
using CrossoutLogView.GUI.Navigation;

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

namespace CrossoutLogView.GUI
{
    /// <summary>
    /// Interaction logic for AuxilaryWindow.xaml
    /// </summary>
    public partial class NavigationWindow : MetroWindow
    {
        private readonly ViewModelBase viewModel;
        public NavigationWindow(ViewModelBase viewModel)
        {
            InitializeComponent();

            DataContext = new NavigationWindowViewModel();

            this.viewModel = viewModel;

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (viewModel is UserModel um) frame.Navigate(new UserPage(frame, um));
            else if (viewModel is GameModel gm) frame.Navigate(new GamePage(frame, gm));
            else if (viewModel is PlayerModel pm) frame.Navigate(new PlayerPage(frame, pm));
            else if (viewModel is UserListModel ul) frame.Navigate(new UserListPage(frame, ul));
            else throw new ArgumentException("View model not supported", nameof(viewModel));
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
    }
}
