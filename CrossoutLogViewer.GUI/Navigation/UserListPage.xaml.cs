using CrossoutLogView.Common;
using CrossoutLogView.Database.Data;
using CrossoutLogView.GUI.Controls;
using CrossoutLogView.GUI.Events;
using CrossoutLogView.GUI.Models;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CrossoutLogView.GUI.Navigation
{
    /// <summary>
    /// Interaction logic for UserListPage.xaml
    /// </summary>
    public partial class UserListPage
    {
        private readonly Frame frame;
        private UserListModel userListViewModel;

        public UserListPage(Frame frame, UserListModel userList)
        {
            this.frame = frame;
            InitializeComponent();
            DataContext = userList;
            userListViewModel = userList;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            UsersListControl.ItemsSource = userListViewModel.Users;
            UsersListControl.FilterUserName = userListViewModel.FilterUserName;
        }
    }
}
