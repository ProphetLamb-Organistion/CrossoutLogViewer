using CrossoutLogView.Common;
using CrossoutLogView.Database.Data;

using JetBrains.Annotations;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace CrossoutLogView.GUI.Core
{
    public interface ICollectionViewModel
    {
        public void UpdateCollectionsSafe();
    }

    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool Set<T>(ref T field, T newValue = default(T), [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
            {
                return false;
            }

            field = newValue;

            OnPropertyChanged(propertyName);

            return true;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public abstract class CollectionViewModel : ViewModelBase, ICollectionViewModel
    {
        protected abstract void UpdateCollections();

        public void UpdateCollectionsSafe()
        {
            try
            {
                UpdateCollections();
            }
            catch (Exception) { }
        }
    }

    public class WindowViewModelBase : CollectionViewModel
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
