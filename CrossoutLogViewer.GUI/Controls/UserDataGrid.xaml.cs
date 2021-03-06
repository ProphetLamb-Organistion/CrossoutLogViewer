﻿using CrossoutLogView.GUI.Core;
using CrossoutLogView.GUI.Events;
using CrossoutLogView.GUI.Helpers;
using CrossoutLogView.GUI.Models;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CrossoutLogView.GUI.Controls
{
    /// <summary>
    /// Interaction logic for UserListView.xaml
    /// </summary>
    public partial class UserDataGrid : ILogging
    {
        public event OpenModelViewerEventHandler OpenViewModel;

        public UserDataGrid()
        {
            InitializeComponent();
            foreach (var column in Columns)
            {
                column.CanUserSort = true;
                column.IsReadOnly = true;
            }
        }

        private void OpenUserMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataGridHelper.GetSourceCellElement(e) is DataGridCell dgc && dgc.DataContext is UserModel u)
            {
                OpenViewModel?.Invoke(e.OriginalSource, new OpenModelViewerEventArgs(u, e));
                e.Handled = true;
            }
        }

        private void OpenUserClick(object sender, RoutedEventArgs e)
        {
            if (SelectedItem is UserModel u)
            {
                OpenViewModel?.Invoke(e.OriginalSource, new OpenModelViewerEventArgs(u, e));
                e.Handled = true;
            }
        }

        #region ILogging support
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        NLog.Logger ILogging.Logger { get; } = logger;
        #endregion
    }
}
