using CrossoutLogView.Common;
using CrossoutLogView.GUI.Events;
using CrossoutLogView.GUI.Services;
using CrossoutLogView.GUI.WindowsAuxilary;

using MahApps.Metro.Controls;

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace CrossoutLogView.GUI.Core
{
    public class ExtenedMetroWindow<TViewModel> : MetroWindow where TViewModel : WindowViewModelBase, new()
    {
        private LoadingWindow loadingWindow = new LoadingWindow();
        protected TViewModel ViewModel { get; private set; }

        /// <summary>
        /// Initializes a new Instance of ExtenedMetroWindow.
        /// Begins the startup behaviour:
        /// 1. [MTA] Call App.InitializeSession
        /// Once done:
        /// 2.  [MTA] Initialize ViewModel
        /// 3.  [MTA] Assign dispatcher to ViewModel
        /// 4.  [SYNC] Call OnInitializeSession
        /// Once done:
        /// 5.  [SYNC] Call OnInitializedViewModel
        /// </summary>
        public ExtenedMetroWindow()
        {
            loadingWindow = new LoadingWindow
            {
                IsIndeterminate = true,
                Title = "Loading"
            };

            CallbackTask.Run(App.InitializeSession, StartUpInitializeSession);

            loadingWindow.Show();
        }

        private void StartUpInitializeSession()
        {
            this.Invoke(delegate
            {
                ResourceManagerService.LocaleChanged += LocaleChanged;
                DataContext = ViewModel = new TViewModel { WindowDispatcher = Dispatcher };
                OnInitializeSession();
                //ViewModel.Initialized += StartUpViewModelInitialized;
            });
        }

        private void StartUpViewModelInitialized(object sender, EventArgs e)
        {
            OnInitializedViewModel();
            loadingWindow.Close();
        }

        protected virtual void LocaleChanged(object sender, ValueChangedEventArgs<Locale> e)
        {
            if (!(e is null || e.NewValue is null))
                FlowDirection = e.NewValue.RTL ? System.Windows.FlowDirection.RightToLeft : System.Windows.FlowDirection.LeftToRight;
        }

        protected virtual void OnInitializeSession() { }
        protected virtual void OnInitializedViewModel() { }
    }
}
