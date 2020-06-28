using CrossoutLogView.GUI.Events;
using CrossoutLogView.GUI.Helpers;

using System;
using System.Collections.Generic;
using System.Text;

namespace CrossoutLogView.GUI.Core
{
    public class StatDisplayViewModeBase : CollectionViewModelBase
    {
        private DisplayMode _statDisplayMode = DisplayMode.GameAvg;
        private static bool lockUpdate = false;

        private static event ValueChangedEventHandler<DisplayMode> StatDisplayModeChanged;

        public StatDisplayViewModeBase()
        {
            StatDisplayModeChanged += OnStatDisplayModeChanged;
        }

        private void OnStatDisplayModeChanged(object sender, ValueChangedEventArgs<DisplayMode> e)
        {
            StatDisplayMode = e.NewValue;
        }

        public DisplayMode StatDisplayMode
        {
            get => _statDisplayMode;
            set
            {
                var oldValue = _statDisplayMode;
                Set(ref _statDisplayMode, value);
                if (!lockUpdate)
                {
                    lockUpdate = true;
                    StatDisplayModeChanged?.Invoke(this, new ValueChangedEventArgs<DisplayMode>(oldValue, value));
                    lockUpdate = false;
                }
                UpdateProperties();
            }
        }

        protected override void UpdateCollections() { }

        public virtual void UpdateProperties() { }
    }
}
