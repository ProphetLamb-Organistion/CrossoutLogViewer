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
    public partial class UserListPage : Page
    {
        private readonly Frame frame;

        public UserListPage(Frame frame, UserListModel userList)
        {
            this.frame = frame;
            InitializeComponent();
            DataContext = userList;
            Object = userList;

            userList.PropertyChanged += OnPropertyChanged;
            userList.Users.Sort(new UserModelParticipationCountDescending());

            var view = (CollectionView)CollectionViewSource.GetDefaultView(UserListViewUsers.ItemsSource = userList.Users);
            view.Filter = UserListFilter;
            view.Refresh();
        }

        private void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(CollectedStatisticsWindowViewModel.UserNameFilter):
                    CollectionViewSource.GetDefaultView(UserListViewUsers.ItemsSource).Refresh();
                    break;
            }
        }

        public UserListModel Object { get; }

        private void UserSelectUser(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as UserDataGrid).SelectedItem is UserModel model)
            {
                UserOverviewUsers.DataContext = new UserModel(model.Object);
            }
        }

        private void UserOpenUserClick(object sender, OpenModelViewerEventArgs e)
        {
            if (e.ViewModel is UserModel user)
            {
                Logging.WriteLine<CollectedStatisticsWindow>("Open user view.");
                frame.Navigate(new UserPage(frame, user));
            }
        }

        private bool UserListFilter(object obj)
        {
            if (String.IsNullOrEmpty(Object.UserNameFilter)) return true;
            if (!(obj is UserModel ul)) return false;
            foreach (var part in Object.UserNameFilter.TrimEnd().Split(' ', '-', '_'))
            {
                if (!ul.Object.Name.Contains(part, StringComparison.InvariantCultureIgnoreCase)) return false;
            }
            return true;
        }
    }
}
