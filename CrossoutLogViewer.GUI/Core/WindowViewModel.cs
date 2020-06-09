using CrossoutLogView.Database.Data;
using CrossoutLogView.GUI.Events;

using System;
using System.Collections.Generic;
using System.Text;

namespace CrossoutLogView.GUI.Core
{
    public class WindowViewModel : ViewModelBase
    {
        private bool _colorWindowTitlebar = Settings.Current.ColorWindowTitlebar;

        public static ColorWindowTitlebarEventHandler ColorWindowTitlebarChanged;

        public WindowViewModel()
        {
            ColorWindowTitlebarChanged += OnColorWindowTitlebarChanged;
        }

        private void OnColorWindowTitlebarChanged(object sender, ColorWindowTitlebarEventArgs e)
        {
            _colorWindowTitlebar = e.NewValue;
            OnPropertyChanged(nameof(ColorWindowTitlebar));
        }

        public bool ColorWindowTitlebar
        {
            get => _colorWindowTitlebar;
            set
            {
                var oldValue = _colorWindowTitlebar;
                Set(ref _colorWindowTitlebar, value);
                Settings.Current.ColorWindowTitlebar = value;
                ColorWindowTitlebarChanged?.Invoke(this, new ColorWindowTitlebarEventArgs(oldValue, value));
            }
        }

        public override void UpdateCollections() { } //override on demand, not required
    }
}
