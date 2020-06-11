using CrossoutLogView.GUI.Core;
using CrossoutLogView.GUI.Events;
using CrossoutLogView.Statistics;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;

namespace CrossoutLogView.GUI.Models
{
    public class UserListModel : ViewModelBase
    {
        public UserListModel(ObservableCollection<UserModel> users)
        {
            Users = users;
        }

        private string _filterUserName;
        public string FilterUserName { get => _filterUserName; set => Set(ref _filterUserName, value?.TrimStart()); }

        public ObservableCollection<UserModel> Users { get; }

        public override void UpdateCollections() { }

    }
}
