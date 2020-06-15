using CrossoutLogView.GUI.Core;
using CrossoutLogView.GUI.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CrossoutLogView.GUI.Controls
{
    /// <summary>
    /// Interaction logic for UserOverview.xaml
    /// </summary>
    public partial class UserOverview : ILogging
    {
        public UserOverview()
        {
            InitializeComponent();
            ComboBoxDisplayMode.ItemsSource = Enum.GetValues(typeof(DisplayMode)).Cast<DisplayMode>();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is UserModel umNew && e.OldValue is UserModel umOld)
            {
                umNew.StatDisplayMode = umOld.StatDisplayMode;
            }
        }

        #region ILogging support
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        NLog.Logger ILogging.Logger { get; } = logger;
        #endregion
    }
}
