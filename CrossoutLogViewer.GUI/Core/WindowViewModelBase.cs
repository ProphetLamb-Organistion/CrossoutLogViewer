using CrossoutLogView.Database.Data;

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Threading;
using System.Threading.Tasks;

namespace CrossoutLogView.GUI.Core
{
    public class WindowViewModelBase : CollectionViewModelBase
    {
        private bool _colorWindowTitlebar = Settings.Current.ColorWindowTitlebar;
        public event EventHandler Initialized;

        public WindowViewModelBase()
        {
            Settings.SettingsPropertyChanged += Settings_SettingsPropertyChanged;
            StartUpInitialize();
        }

        public bool IsInitialized { get; private set; }

        public Dispatcher WindowDispatcher { get; internal set; }

        public bool ColorWindowTitlebar
        {
            get => _colorWindowTitlebar;
            set
            {
                Set(ref _colorWindowTitlebar, value);
                Settings.Current.ColorWindowTitlebar = value;
            }
        }

        private void Settings_SettingsPropertyChanged(Settings sender, Database.Events.SettingsChangedEventArgs e)
        {
            if (sender != null && e != null && e.Name == nameof(Settings.ColorWindowTitlebar))
            {
                _colorWindowTitlebar = (bool)e.NewValue;
                OnPropertyChanged(nameof(ColorWindowTitlebar));
            }
        }

        private async void StartUpInitialize()
        {
            if (IsInitialized) return;
            await Task.Run(OnInitialize);
            IsInitialized = true;
            Initialized?.Invoke(this, new EventArgs());
        }

        protected virtual void OnInitialize()
        {
            UpdateCollectionsSafe();
        }

        protected override void UpdateCollections() { }
    }
}
