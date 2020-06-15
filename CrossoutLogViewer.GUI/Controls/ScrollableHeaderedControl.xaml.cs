using CrossoutLogView.GUI.Core;

using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    [DefaultProperty(nameof(Content))]
    /// <summary>
    /// Interaction logic for ScrollableHeaderControl.xaml
    /// </summary>
    public partial class ScrollableHeaderedControl : ILogging
    {
        public ScrollableHeaderedControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the header used to generate the content of the <see cref="ScrollableHeaderedControl"/>.
        /// </summary>
        public object HeaderContent { get => GetValue(HeaderContentProperty); set => SetValue(HeaderContentProperty, value); }
        public static readonly DependencyProperty HeaderContentProperty = DependencyProperty.Register(nameof(HeaderContent), typeof(object), typeof(ScrollableHeaderedControl));

        /// <summary>
        /// Gets or sets the content used to generate the content of the <see cref="ScrollableHeaderedControl"/>.
        /// </summary>
        public new object Content { get => GetValue(ContentProperty); set => SetValue(ContentProperty, value); }
        public static new readonly DependencyProperty ContentProperty = DependencyProperty.Register(nameof(Content), typeof(object), typeof(ScrollableHeaderedControl));

        private void ContentPresenter_Content_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var args = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
            args.RoutedEvent = ScrollViewer.MouseWheelEvent;
            ScrollViewerMain.RaiseEvent(args);
        }

        #region ILogging support
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        NLog.Logger ILogging.Logger { get; } = logger;
        #endregion
    }
}
