using CrossoutLogView.Common;

using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CrossoutLogView.GUI
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow
    {
        private static string deleteDatabaseConfirmation = String.Concat("This will delete all colleted data. We might not be able to completely restore the data, because Crossout deletes logs after a certain time period.", Environment.NewLine, "Afterwards this application will shutdown.  Are you sure that you wish to proceed?");
        private SettingsWindowViewModel viewModel = new SettingsWindowViewModel();

        public SettingsWindow()
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void ChangeThemeClick(object sender, RoutedEventArgs e)
        {
            viewModel.AccentColor.ChangeAccentCommand.Execute(sender);
            viewModel.AppTheme.ChangeAccentCommand.Execute(sender);
            SettingsWindowViewModel.ApplyColors();
        }

        private void ResetColorsClick(object sender, RoutedEventArgs e)
        {
            viewModel.ResetColors();
        }

        private void OpenSettingsFileClick(object sender, RoutedEventArgs e)
        {
            ExplorerOpenFile.OpenFile(Strings.DataBaseCurrentSettingsPath);
        }

        private void OpenEventLogClick(object sender, RoutedEventArgs e)
        {
            ExplorerOpenFile.OpenFile(Strings.DataBaseEventLogPath);
        }

        private async void DeleteDatabaseClick(object sender, RoutedEventArgs e)
        {
            var settings = new MetroDialogSettings
            {
                AnimateHide = false,
                AnimateShow = false,
                AffirmativeButtonText = "Proceed",
                NegativeButtonText = "Cancel",
                MaximumBodyHeight = 150,
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
