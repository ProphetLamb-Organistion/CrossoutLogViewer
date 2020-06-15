using CrossoutLogView.GUI.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrossoutLogView.GUI.Models
{
    public class UsersListControlModel : ViewModelBase
    {
        private string _userName;
        private string[] _filtersUserName;

        public string FilterUserName { 
            get => _userName; 
            set
            {
                var val = value?.TrimStart();
                if (String.IsNullOrWhiteSpace(val))
                    FiltersUserName = Array.Empty<string>();
                else
                    FiltersUserName = val.Split('|').Where(x => !String.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToArray();
                Set(ref _userName, val);
            }
        }

        public string[] FiltersUserName { get => _filtersUserName; set => Set(ref _filtersUserName, value); }
    }
}
