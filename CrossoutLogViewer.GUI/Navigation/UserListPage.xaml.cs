using CrossoutLogView.Common;
using CrossoutLogView.Database.Data;
using CrossoutLogView.GUI.Controls;
using CrossoutLogView.GUI.Core;
using CrossoutLogView.GUI.Events;
using CrossoutLogView.GUI.Models;
using CrossoutLogView.GUI.WindowsAuxilary;
using CrossoutLogView.Statistics;

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

namespace CrossoutLogView.GUI.Navigation
{
    /// <summary>
    /// Interaction logic for UserListPage.xaml
    /// </summary>
    public partial class UserListPage : ILogging
    {
        private readonly NavigationWindow nav;
        private UserListModel userListViewModel;

        public UserListPage(NavigationWindow nav, UserListModel userList)
        {
            this.nav = nav ?? throw new ArgumentNullException(nameof(nav));
            InitializeComponent();
            DataContext = userList;
            userListViewModel = userList ?? throw new ArgumentNullException(nameof(userList));
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            UsersListControl.ItemsSource = userListViewModel.Users;
            UsersListControl.FilterUserName = userListViewModel.FilterUserName;
        }

        private void UsersListControl_OpenViewModel(object sender, OpenModelViewerEventArgs e)
        {
            if (e.ViewModel is UserModel viewModel)
            {
                nav.Navigate(new UserPage(nav, viewModel));
            }
        }

        #region ILogging support
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        NLog.Logger ILogging.Logger { get; } = logger;
        #endregion
    }
}
