using CrossoutLogView.Database.Data;
using CrossoutLogView.GUI.Models;
using CrossoutLogView.GUI.WindowsAuxilary;
using LiveCharts;
using LiveCharts.Wpf;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
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
    public partial class PlayerGamesChart : UserControl
    {
        private PlayerGamesChartModel viewModel;

        public PlayerGamesChart()
        {
            InitializeComponent();
            DataContext = viewModel = new PlayerGamesChartModel();
            MultiFlagComboBoxDimensions.LoadEnumValues<Dimensions>();
            MultiFlagComboBoxDimensions.SelectedValue = (Dimensions)Settings.Current.Dimensions;
        }

        public ObservableCollection<PlayerGameCompositeModel> ItemsSource
        {
            get => GetValue(ItemsSourceProperty) as ObservableCollection<PlayerGameCompositeModel>;
            set
            {
                viewModel.Source = value;
                SetValue(ItemsSourceProperty, value);
            }
        }
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(nameof(ItemsSource), typeof(ObservableCollection<PlayerGameCompositeModel>), typeof(PlayerGamesChart));

        public Dimensions Dimensions 
        {
            get => (Dimensions)GetValue(DimensionsProperty);
            set
            {
                Settings.Current.Dimensions = (int)value;
                if (IsLoaded)
                {
                    viewModel.SeriesDimensions = value;
                }
                SetValue(DimensionsProperty, value);
            }
        }
        public static readonly DependencyProperty DimensionsProperty = DependencyProperty.Register(nameof(Dimensions), typeof(Dimensions), typeof(PlayerGamesChart), new PropertyMetadata((Dimensions)Settings.Current.Dimensions));

        private void CartesianChart_DataClick(object sender, ChartPoint chartPoint)
        {
            new NavigationWindow(ItemsSource[(int)Math.Round(chartPoint.X)].Game).ShowDialog();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            viewModel.SeriesDimensions = Dimensions;
            viewModel.UpdateCollections();
        }

        private void RangeSlider_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is RangeSlider slider)
            {
                slider.LowerValue = slider.Minimum;
                slider.UpperValue = slider.Maximum;
            }
        }

        private void MultiFlagComboBox_SelectedValueChanged(object sender, RoutedPropertyChangedEventArgs<NamedEnum> e)
        {
            Dimensions = (Dimensions)e.NewValue.Value;
        }
    }
}
