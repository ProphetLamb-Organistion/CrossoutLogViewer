using CrossoutLogView.Common;
using CrossoutLogView.Database.Data;
using CrossoutLogView.GUI.Core;
using CrossoutLogView.GUI.Events;
using CrossoutLogView.GUI.Helpers;
using CrossoutLogView.GUI.Models;

using LiveCharts;
using LiveCharts.Wpf;

using MahApps.Metro.Controls;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CrossoutLogView.GUI.Controls
{
    /// <summary>
    /// Interaction logic for PlayerGamesChart.xaml
    /// </summary>
    public partial class PlayerGamesChart : ILogging
    {
        private PlayerGamesChartModel viewModel;
        public event OpenModelViewerEventHandler OpenViewModel;

        public PlayerGamesChart()
        {
            InitializeComponent();
            DataContext = viewModel = new PlayerGamesChartModel((Content as Grid).FindChild<CartesianChart>());
            MultiFlagComboBoxDimensions.LoadEnumValues<Dimensions>();
            MultiFlagComboBoxDimensions.SelectedValue = (Dimensions)Settings.Current.Dimensions;

            Settings.SettingsPropertyChanged += Settings_SettingsPropertyChanged;
        }

        private void Settings_SettingsPropertyChanged(Settings sender, Database.Events.SettingsChangedEventArgs e)
        {
            if (sender != null && e != null && e.Name == nameof(Settings.Dimensions) && sender.Dimensions != (int)Dimensions)
            {
                Dimensions = (Dimensions)sender.Dimensions;
            }
        }

        public ObservableCollection<PlayerGameModel> ItemsSource
        {
            get => GetValue(ItemsSourceProperty) as ObservableCollection<PlayerGameModel>;
            set => SetValue(ItemsSourceProperty, value);
        }
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(nameof(ItemsSource), typeof(ObservableCollection<PlayerGameModel>), typeof(PlayerGamesChart), new PropertyMetadata(OnItemsSourcePropertyChanged));

        public Dimensions Dimensions
        {
            get => (Dimensions)GetValue(DimensionsProperty);
            set
            {
                Settings.Current.Dimensions = (int)value;
                if (IsLoaded)
                    viewModel.SeriesDimensions = value;
                SetValue(DimensionsProperty, value);
            }
        }
        public static readonly DependencyProperty DimensionsProperty = DependencyProperty.Register(nameof(Dimensions), typeof(Dimensions), typeof(PlayerGamesChart), new PropertyMetadata((Dimensions)Settings.Current.Dimensions));

        private static void OnItemsSourcePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is PlayerGamesChart cntr && e.NewValue is ObservableCollection<PlayerGameModel> newValue)
            {
                cntr.viewModel.Source = newValue;
            }
        }

        private void CartesianChart_DataClick(object sender, ChartPoint chartPoint)
        {
            var viewModel = ItemsSource[(int)Math.Round(chartPoint.X)].Game;
            if (viewModel != null)
                OpenViewModel?.Invoke(this, new OpenModelViewerEventArgs(viewModel));
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            viewModel.SeriesDimensions = Dimensions;
            viewModel.UpdateCollectionsSafe();
        }

        private void RangeSlider_MouseDoubleClick(object sender, MouseButtonEventArgs e) => ResetZoom();
        public void ResetZoom()
        {
            RangeSlider_Zoom.LowerValue = RangeSlider_Zoom.Minimum;
            RangeSlider_Zoom.UpperValue = RangeSlider_Zoom.Maximum;
        }

        private void MultiFlagComboBox_SelectedValueChanged(object sender, RoutedPropertyChangedEventArgs<NamedEnum> e)
        {
            Dimensions = (Dimensions)e.NewValue.Value;
        }

        #region ILogging support
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        NLog.Logger ILogging.Logger { get; } = logger;
        #endregion
    }
}
