using ControlzEx.Theming;
using CrossoutLogView.Common;
using CrossoutLogView.Database;
using CrossoutLogView.Database.Collection;
using CrossoutLogView.Database.Connection;
using CrossoutLogView.Database.Data;
using CrossoutLogView.Database.Events;
using CrossoutLogView.GUI.Controls;
using CrossoutLogView.GUI.Core;
using CrossoutLogView.GUI.Events;
using CrossoutLogView.GUI.Models;
using CrossoutLogView.Statistics;

using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;

namespace CrossoutLogView.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private MainWindowViewModel viewModel;

        public MainWindow()
        {
            App.InitializeEnvironment();

            Logging.WriteLine<MainWindow>("Loading main UserInterface", true);
            InitializeComponent();

            viewModel = new MainWindowViewModel();
            viewModel.PropertyChanged += OnPropertyChanged;
            DataContext = viewModel;

            UserGamesViewGames.User = MainWindowViewModel.MeUser;
            UserGamesViewGames.DataGridGames.OpenViewModel += GamesOpenGameDoubleClick;

            UserListViewUsers.ItemsSource = MainWindowViewModel.UserListModels;
            var userListView = (CollectionView)CollectionViewSource.GetDefaultView(UserListViewUsers.ItemsSource);
            userListView.Filter = UserListFilter;

            WeaponListViewWeapons.ItemsSource = MainWindowViewModel.WeaponModels;

            MapsView.Maps = MainWindowViewModel.Maps;
            MapsView.PlayerGamesDataGrid.OpenViewModel += GamesOpenGameDoubleClick;

            MainWindowViewModel.InvalidatedCachedData += OnInvalidateCachedData;

            Loaded += OnLoad;
        }

        private void OnInvalidateCachedData(object sender, InvalidateCachedDataEventArgs e)
        {
            this.BeginInvoke(delegate
            {
                UserListViewUsers.ItemsSource = MainWindowViewModel.UserListModels;
                CollectionViewSource.GetDefaultView(UserListViewUsers.ItemsSource).Refresh();
                WeaponListViewWeapons.ItemsSource = MainWindowViewModel.WeaponModels;
            });
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            Title = String.Concat(Title, " (", MainWindowViewModel.MeUser.Object.Name, ")");
            HamburgerMenuControl.Focus();

            Logging.WriteLine<MainWindow>("Main UserInterface loaded in {TP}");
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(MainWindowViewModel.UserNameFilter):
                    CollectionViewSource.GetDefaultView(UserListViewUsers.ItemsSource).Refresh();
                    break;
            }
        }

        private void HamburgerMenuControl_ItemInvoked(object sender, HamburgerMenuItemInvokedEventArgs args)
        {
            if (args.InvokedItem is HamburgerMenuItem hmi && hmi.Label == "Settings")
            {
                ContentTabControl.SelectedItem = TabItemSettings;
            }
            else
            {
                ContentTabControl.SelectedIndex = HamburgerMenuControl.SelectedIndex;
            }
            HamburgerMenuControl.IsPaneOpen = false;
        }

        private void GamesOpenGameDoubleClick(object sender, OpenModelViewerEventArgs e)
        {
            if (e.ViewModel is PlayerGameCompositeModel pgc)
            {
                Logging.WriteLine<MainWindow>("Open game view.");
                new NavigationWindow(pgc.Game).ShowDialog();
            }
            else if (e.ViewModel is UserListModel ul)
            {
                Logging.WriteLine<MainWindow>("Open user list.");
                new NavigationWindow(ul).ShowDialog();
            }
        }

        private void UserSelectUser(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as UserDataGrid).SelectedItem is UserModel model)
            {
                UserOverviewUsers.DataContext = new UserModel(model.Object);
            }
        }

        private bool UserListFilter(object obj)
        {
            if (String.IsNullOrEmpty(viewModel.UserNameFilter)) return true;
            if (!(obj is UserModel ul)) return false;
            foreach (var part in viewModel.UserNameFilter.TrimEnd().Split(' ', '-', '_'))
            {
                if (!ul.Object.Name.Contains(part, StringComparison.InvariantCultureIgnoreCase)) return false;
            }
            return true;
        }

        private void UserOpenUserDoubleClick(object sender, OpenModelViewerEventArgs e)
        {
            if (e.ViewModel is UserModel user)
            {
                Logging.WriteLine<MainWindow>("Open user view.");
                new NavigationWindow(new UserModel(user.Object)).ShowDialog();
            }
        }

        private void WeaponOpenUserDoubleClick(object sender, OpenModelViewerEventArgs e)
        {
            if (e.ViewModel is WeaponUserListModel weapon)
            {
                Logging.WriteLine<MainWindow>("Open user view.");
                new NavigationWindow(new UserModel(weapon.User)).ShowDialog();
            }
        }

        private void ChangeThemeClick(object sender, RoutedEventArgs e)
        {
            viewModel.AccentColor.ChangeAccentCommand.Execute(sender);
            viewModel.AppTheme.ChangeAccentCommand.Execute(sender);
            MainWindowViewModel.ApplyColors();
        }

        private void ResetColorsClick(object sender, RoutedEventArgs e)
        {
            viewModel.ResetColors();
        }

        private void CloseHamburgerMenuPane(object sender, RoutedEventArgs e)
        {
            HamburgerMenuControl.IsPaneOpen = false;
        }

        private void OpenSettingsFileClick(object sender, RoutedEventArgs e)
        {
            ExplorerOpenFile.OpenFile(Strings.DataBaseCurrentSettingsPath);
        }

        private void OpenEventLogClick(object sender, RoutedEventArgs e)
        {
            ExplorerOpenFile.OpenFile(Strings.DataBaseEventLogPath);
        }

        private static string deleteDatabaseConfirmation = String.Concat("This will delete all colleted data. We might not be able to completely restore the data, because Crossout deletes logs after a certain time period.", Environment.NewLine, "Afterwards this application will shutdown.  Are you sure that you wish to proceed?");
        private async void DeleteDatabaseClick(object sender, RoutedEventArgs e)
        {
            var settings = new MetroDialogSettings
            {
                AffirmativeButtonText = "Proceed",
                NegativeButtonText = "Cancel",
                MaximumBodyHeight = 300,
                ColorScheme = MetroDialogOptions.ColorScheme
            };
            MessageDialogResult result = await this.ShowMessageAsync(
                "Delete database confirmation",
                deleteDatabaseConfirmation,
                MessageDialogStyle.AffirmativeAndNegative,
                settings);
            if (result == MessageDialogResult.Affirmative)
            {
                App.SessionControlService.DeleteDatabase();
                Application.Current.Shutdown();
            }
        }
    }
}
