using CrossoutLogView.GUI.Controls.SessionCalendar;
using CrossoutLogView.GUI.Core;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;

namespace CrossoutLogView.GUI.Models
{
    public class SessionMonthModel : ViewModelBase
    {
        private ObservableCollection<DateWeek> _weeks;
        public ObservableCollection<DateWeek> Weeks { get => _weeks; set => Set(ref _weeks, value); }
    }

    public class DateWeek : ViewModelBase
    {
        private DateTime _startOfWeek = default, _endOfWeek = default;
        public DateTime StartOfWeek { get => _startOfWeek; set => Set(ref _startOfWeek, value); }
        public DateTime EndOfWeek { get => _endOfWeek; set => Set(ref _endOfWeek, value); }
    }
}
