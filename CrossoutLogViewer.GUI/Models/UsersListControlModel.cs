using CrossoutLogView.GUI.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrossoutLogView.GUI.Models
{
    public class UsersListControlModel : ViewModelBase
    {
        private string _userName;
        public string FilterUserName { get => _userName; set => Set(ref _userName, value?.TrimStart()); }

        public override void UpdateCollections() => throw new InvalidOperationException();
    }
}
