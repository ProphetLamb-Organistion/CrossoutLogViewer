using CrossoutLogView.GUI.Core;
using CrossoutLogView.GUI.Events;
using CrossoutLogView.Statistics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace CrossoutLogView.GUI.Models
{
    public class UserListModel : ViewModelBase
    {
        public UserListModel(List<UserModel> users)
        {
            Users = users;
        }

        private string _userNameFilter;
        public string UserNameFilter { get => _userNameFilter; set => Set(ref _userNameFilter, value?.TrimStart()); }

        public List<UserModel> Users { get; }

        public override void UpdateCollections() { }

    }
}
