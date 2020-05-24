using CrossoutLogView.Log;

using System;
using System.Collections.Generic;
using System.Text;

namespace CrossoutLogView.Database.Events
{
    public delegate void LogUploadEventEventHandler(object sender, LogUploadEventArgs e);
    public sealed class LogUploadEventArgs : EventArgs
    {
        public LogUploadEventArgs(DateTime oldNewestLogEntryDateTime, DateTime newNewestLogEntryDateTime)
        {
            OldNewestLogEntryDateTime = oldNewestLogEntryDateTime;
            NewNewestLogEntryDateTime = newNewestLogEntryDateTime;
        }

        public DateTime OldNewestLogEntryDateTime { get; }

        public DateTime NewNewestLogEntryDateTime { get; }
    }
}
