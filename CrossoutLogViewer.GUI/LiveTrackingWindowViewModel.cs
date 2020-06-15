using CrossoutLogView.GUI.Core;
using CrossoutLogView.GUI.Models;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace CrossoutLogView.GUI
{
    public class LiveTrackingWindowViewModel : WindowViewModelBase
    {
        private bool initialized = false;

        public LiveTrackingWindowViewModel() : base() { }

        public LiveTrackingWindowViewModel(Dispatcher dispatcher) : base()
        {
            WindowDispatcher = dispatcher;
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            SettingsWindowViewModel.ApplyColors();
        }

        protected override void UpdateCollections()
        {

        }
    }
}
