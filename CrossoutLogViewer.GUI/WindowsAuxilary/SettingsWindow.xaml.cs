using ControlzEx.Standard;

using CrossoutLogView.Common;
using CrossoutLogView.GUI.Core;
using CrossoutLogView.GUI.Models;

using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
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
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : MetroWindow, ILogging
    {
        private SettingsWindowViewModel viewModel;

        public SettingsWindow()
        {
            logger.TraceResource("WinInit");
            InitializeComponent();
            DataContext = viewModel = new SettingsWindowViewModel();
            logger.TraceResource("WinInitD");
        }

        private void ChangeThemeClick(object sender, RoutedEventArgs e)
        {
            viewModel.AccentColor.ChangeAccentCommand.Execute(sender);
            viewModel.AppTheme.ChangeAccentCommand.Execute(sender);
            SettingsWindowViewModel.ApplyColors();
            logger.TraceResource("Sett_ChangeTheme");
            e.Handled = true;
        }

        private void ResetColorsClick(object sender, RoutedEventArgs e)
        {
            viewModel.ResetColors();
            e.Handled = true;
        }

        private void OpenSettingsFileClick(object sender, RoutedEventArgs e)
        {
            ExplorerOpenFile.OpenFile(Strings.DataBaseCurrentSettingsPath);
            e.Handled = true;
        }

        private void OpenEventLogClick(object sender, RoutedEventArgs e)
        {
            ExplorerOpenFile.OpenFile(@".\event.log");
            e.Handled = true;
        }

        private async void DeleteDatabaseClick(object sender, RoutedEventArgs e)
        {
            var settings = new MetroDialogSettings
            {
                AnimateHide = false,
                AnimateShow = false,
                AffirmativeButtonText = "Proceed",
                NegativeButtonText = "Cancel",
                MaximumBodyHeight = 500,
                ColorScheme = MetroDialogOptions.ColorScheme
            };
            MessageDialogResult result = await this.ShowMessageAsync(
                App.GetWindowResource("Sett_DelDB_Header"),
                App.GetWindowResource("Sett_DelDB_Message"),
                MessageDialogStyle.AffirmativeAndNegative,
                settings);
            if (result == MessageDialogResult.Affirmative)
            {
                App.SessionControlService.DeleteDatabase();
                Environment.Exit(0);
            }
            e.Handled = true;
        }

        private void MetroWindow_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.GetPosition(this).Y <= TitleBarHeight) //prevent maximize
                e.Handled = true;
        }
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            }
            catch (InvalidOperationException) { }
            catch (Win32Exception) { }
            e.Handled = true;
        }

        private void UpdateClick(object sender, RoutedEventArgs e)
        {
            if (File.Exists(Strings.UpdaterFilePath))
            {

                Process.Start(new ProcessStartInfo
                {
                    FileName = Strings.UpdaterFilePath,
                    Arguments = "UPDATE_LOCAL"
                });
            }
            e.Handled = true;
        }

        #region ILogging support
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        NLog.Logger ILogging.Logger { get; } = logger;
        #endregion

    }
}
