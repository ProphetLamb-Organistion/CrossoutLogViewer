using CrossoutLogView.Common;
using CrossoutLogView.Database.Data;
using CrossoutLogView.Database.Reflection;
using CrossoutLogView.GUI.Core;

using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Definitions.Series;
using LiveCharts.Wpf;
using LiveCharts.Wpf.Charts.Base;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

using static CrossoutLogView.Common.Strings;

namespace CrossoutLogView.GUI.Models
{
    public class PlayerGamesChartModel : CollectionViewModel
    {
        private ObservableCollection<PlayerGameCompositeModel> _source;
        private Dimensions _seriesDimensions;
        private SeriesCollection _series;
        private AxesCollection _axisYCollection;
        private double _axisXMinValue, _axisXMaxValue, _axisXLowerValue, _axisXUpperValue;
        private AxesCollection _axisXCollection;
        private static readonly VariableInfo[] chartValues;
        private static readonly VariableInfo[] playerGameModelVariables;

        static PlayerGamesChartModel()
        {
            var chartValuesInterface = typeof(IChartValues);
            chartValues = VariableInfo.FromType(typeof(PlayerGamesChartModel), true).Where(vi => chartValuesInterface.IsAssignableFrom(vi.VariableType)).ToArray();
            playerGameModelVariables = VariableInfo.FromType(typeof(PlayerGameCompositeModel), true);
        }

        public PlayerGamesChartModel(Chart chart)
        {
            Chart = chart ?? throw new ArgumentNullException(nameof(chart));
        }

        public Chart Chart { get; }

        public ObservableCollection<PlayerGameCompositeModel> Source
        {
            get => _source;
            set
            {
                if (_source != null)
                    _source.CollectionChanged -= Source_CollectionChanged;
                Set(ref _source, value);
                if (_source != null)
                    _source.CollectionChanged += Source_CollectionChanged;
                UpdateCollectionsSafe();
            }
        }

        public Dimensions SeriesDimensions
        {
            get => _seriesDimensions;
            set
            {
                Set(ref _seriesDimensions, value);
                UpdateSeriesDimensions();
            }
        }

        public SeriesCollection Series { get => _series; private set => Set(ref _series, value); }

        public AxesCollection AxisYCollection { get => _axisYCollection; private set => Set(ref _axisYCollection, value); }
        public AxesCollection AxisXCollection { get => _axisXCollection; private set => Set(ref _axisXCollection, value); }

        public double AxisXMinValue { get => _axisXMinValue; protected set => Set(ref _axisXMinValue, value); }
        public double AxisXMaxValue { get => _axisXMaxValue; protected set => Set(ref _axisXMaxValue, value); }
        public double AxisXLowerValue { get => _axisXLowerValue; set => Set(ref _axisXLowerValue, value); }
        public double AxisXUpperValue { get => _axisXUpperValue; set => Set(ref _axisXUpperValue, value); }

        public ChartValues<int> Score { get; protected set; } = new ChartValues<int>();
        public ChartValues<int> Kills { get; protected set; } = new ChartValues<int>();
        public ChartValues<int> Assists { get; protected set; } = new ChartValues<int>();
        public ChartValues<int> Deaths { get; protected set; } = new ChartValues<int>();
        public ChartValues<double> ArmorDamageDealt { get; protected set; } = new ChartValues<double>();
        public ChartValues<double> CriticalDamageDealt { get; protected set; } = new ChartValues<double>();
        public ChartValues<double> ArmorDamageTaken { get; protected set; } = new ChartValues<double>();
        public ChartValues<double> CriticalDamageTaken { get; protected set; } = new ChartValues<double>();
        public ChartValues<double> TotalDamageDealt { get; protected set; } = new ChartValues<double>();
        public ChartValues<double> TotalDamageTaken { get; protected set; } = new ChartValues<double>();

        private void InitializeSeries()
        {
            //series & yaxis
            Series = new SeriesCollection();
            AxisYCollection = new AxesCollection();
            var series = new LineSeries[chartValues.Length];
            var yAxes = new Axis[chartValues.Length];
            for (int i = 0; i < chartValues.Length; i++)
            {
                series[i] = SeriesViewFactory(chartValues[i].GetValue(this) as IChartValues, chartValues[i].Name, i);
                yAxes[i] = new Axis { ShowLabels = false };
            }
            Series.AddRange(series);
            AxisYCollection.AddRange(yAxes);
            //label formatter
            var xAxis = new Axis
            {
                LabelsRotation = 1,
                LabelFormatter = Formatter
            };
            //bind x axis scale
            xAxis.SetBinding(Axis.MaxValueProperty, new Binding("AxisXUpperValue") { Mode = BindingMode.OneWay });
            xAxis.SetBinding(Axis.MinValueProperty, new Binding("AxisXLowerValue") { Mode = BindingMode.OneWay });
            AxisXCollection = new AxesCollection { xAxis };

            Chart.SetBinding(Chart.AxisXProperty, new Binding("AxisXCollection") { Mode = BindingMode.OneWay });
            Chart.SetBinding(Chart.AxisYProperty, new Binding("AxisYCollection") { Mode = BindingMode.OneWay });
        }

        public string Formatter(double x)
        {
            var index = (int)Math.Round(x); // Convert x axis value to index
            if (index < 0 || index >= Source.Count) // Check for argument out of range
                return String.Empty;
            return DateTimeStringFactory(Source[index].StartTime); 
        }

        protected override void UpdateCollections()
        {
            if (Source == null)
                return;
            // X axis Bounderies cannot have a negative range
            AxisXMinValue = AxisXLowerValue = 0;
            AxisXMaxValue = AxisXUpperValue = Source.Count == 0 ? 0 : Source.Count - 1;
            if (Series == null)
                InitializeSeries();
            for (int i = 0; i < chartValues.Length; i++)
            {
                var target = chartValues[i].GetValue(this); // Instance of ChartValues property of the ViewModel
                chartValues[i].VariableType.GetMethod("Clear").Invoke(target, null); // Clear method of target. Invoked
                var src = playerGameModelVariables.FirstOrDefault(x => x.Name == chartValues[i].Name); // Property in source corrosponding to property target
                var add = chartValues[i].VariableType.GetMethod("Add"); // Add method of target
                for (int j = 0; j < Source.Count; j++)
                {
                    add.Invoke(target, new object[] { src.GetValue(Source[j]) });
                }
            }
            UpdateSeriesDimensions();
        }

        private void UpdateSeriesDimensions()
        {
            if (Source == null)
                return;
            for (int i = 0; i < chartValues.Length; i++)
            {
                var enumValue = (Dimensions)Enum.Parse(typeof(Dimensions), chartValues[i].Name, true); // Enum value associated with chartvalues[i]
                var visibility = (SeriesDimensions & enumValue) == enumValue ? Visibility.Visible : Visibility.Hidden;
                (Series[i] as Series).Visibility = visibility;
                AxisYCollection[i].Visibility = visibility;
            }
        }

        private static LineSeries SeriesViewFactory(IChartValues values, string name, int scaleIndex)
        {
            var series = new LineSeries()
            {
                LineSmoothness = 0.5,
                Values = values,
                Title = name,
                ScalesYAt = scaleIndex,
                DataLabels = false,
                PointGeometrySize = 12,
            };
            return series;
        }

        private void Source_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateCollectionsSafe();
        }
    }
}
