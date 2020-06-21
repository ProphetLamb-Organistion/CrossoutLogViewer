using CrossoutLogView.GUI.Controls.SessionCalendar;
using CrossoutLogView.GUI.Core;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;

namespace CrossoutLogView.GUI.Models
{
    public class SessionWeekModel : ViewModelBase
    {
        private ObservableCollection<DateTime> _days;
        public ObservableCollection<DateTime> Days { get => _days; set => Set(ref _days, value); }
    }
}
