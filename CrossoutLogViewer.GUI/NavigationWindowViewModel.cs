using CrossoutLogView.Database.Data;
using CrossoutLogView.GUI.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrossoutLogView.GUI
{
    internal sealed class NavigationWindowViewModel : ViewModelBase
    {
        public bool ColorWindowTitlebar => Settings.Current.ColorWindowTitlebar;

        public override void UpdateCollections() { }
    }
}
