using CrossoutLogView.Common;
using CrossoutLogView.Database.Data;
using CrossoutLogView.Database.Reflection;
using CrossoutLogView.GUI.Core;
using LiveCharts;
using LiveCharts.Definitions.Series;
using LiveCharts.Wpf;
using LiveCharts.Wpf.Charts.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace CrossoutLogView.GUI.Models
{
    public class PlayerGamesChartModel : ViewModelBase
    {
        private static readonly VariableInfo[] chartValues;
        private static readonly VariableInfo[] playerGameModelVariables;

        private ChartValues<double> _score = new ChartValues<double>();
        private ChartValues<int> _kills = new ChartValues<int>();
        private ChartValues<int> _assists = new ChartValues<int>();
        private ChartValues<int> _deaths = new ChartValues<int>();
        private ChartValues<double> _armorDamageDealt = new ChartValues<double>();
        private ChartValues<double> _criticalDamageDealt = new ChartValues<double>();
        private ChartValues<double> _armorDamageTaken = new ChartValues<double>();
        private ChartValues<double> _criticalDamageTaken = new ChartValues<double>();
        private ChartValues<double> _totalDamageDealt = new ChartValues<double>();
        private ChartValues<double> _totalDamageTaken = new ChartValues<double>();
        private Dimensions _seriesDimensions;
        private ObservableCollection<PlayerGameCompositeModel> _source;
        private SeriesCollection _series = new SeriesCollection();
        private AxesCollection _axisYCollection = new AxesCollection();
        private string[] _labels;
        private double _axisXMinValue, _axisXMaxValue, _axisXLowerValue, _axisXUpperValue;

        static PlayerGamesChartModel()
        {
            var chartValuesInterface = typeof(IChartValues);
            chartValues = VariableInfo.FromType(typeof(PlayerGamesChartModel), true).Where(vi => chartValuesInterface.IsAssignableFrom(vi.VariableType)).ToArray();
            playerGameModelVariables = VariableInfo.FromType(typeof(PlayerGameCompositeModel), true);
        }

        public PlayerGamesChartModel()
        {
            for (int i = 0; i < chartValues.Length; i++)
            {
                var axis = new Axis();
                axis.Separator.IsEnabled = false;
                axis.IsMerged = false;
                axis.ShowLabels = false;
                AxisYCollection.Add(axis);
            }
        }

        public SeriesCollection Series { get => _series; private set => Set(ref _series, value); }

        public string[] Labels { get => _labels; protected set => Set(ref _labels, value); }

        public double AxisXMinValue { get => _axisXMinValue; protected set => Set(ref _axisXMinValue, value); }
        public double AxisXMaxValue { get => _axisXMaxValue; protected set => Set(ref _axisXMaxValue, value); }
        public double AxisXLowerValue { get => _axisXLowerValue; set => Set(ref _axisXLowerValue, value); }
        public double AxisXUpperValue { get => _axisXUpperValue; set => Set(ref _axisXUpperValue, value); }

        public Dimensions SeriesDimensions
        {
            get => _seriesDimensions;
            set
            {
                Set(ref _seriesDimensions, value);
                UpdateSeriesDimensions();
            }
        }
        public ObservableCollection<PlayerGameCompositeModel> Source
        {
            get => _source;
            set
            {
                Set(ref _source, value);
                UpdateCollections();
            }
        }
        public AxesCollection AxisYCollection { get => _axisYCollection; private set => Set(ref _axisYCollection, value); }

        public ChartValues<double> Score { get => _score; private set => Set(ref _score, value); }
        public ChartValues<int> Kills { get => _kills; private set => Set(ref _kills, value); }
        public ChartValues<int> Assists { get => _assists; private set => Set(ref _assists, value); }
        public ChartValues<int> Deaths { get => _deaths; private set => Set(ref _deaths, value); }
        public ChartValues<double> ArmorDamageDealt { get => _armorDamageDealt; private set => Set(ref _armorDamageDealt, value); }
        public ChartValues<double> CriticalDamageDealt { get => _criticalDamageDealt; private set => Set(ref _criticalDamageDealt, value); }
        public ChartValues<double> ArmorDamageTaken { get => _armorDamageTaken; private set => Set(ref _armorDamageTaken, value); }
        public ChartValues<double> CriticalDamageTaken { get => _criticalDamageTaken; private set => Set(ref _criticalDamageTaken, value); }
        public ChartValues<double> TotalDamageDealt { get => _totalDamageDealt; private set => Set(ref _totalDamageDealt, value); }
        public ChartValues<double> TotalDamageTaken { get => _totalDamageTaken; private set => Set(ref _totalDamageTaken, value); }

        public override void UpdateCollections()
        {
            if (Source == null)
                return;
            //series
            using var em = Source.GetEnumerator();
            for (int i = 0; i < chartValues.Length; i++)
            {
                var target = chartValues[i].GetValue(this); // instance of ChartValues property of the ViewModel
                chartValues[i].VariableType.GetMethod("Clear").Invoke(target, null); //clear method of target. Invoked
                var add = chartValues[i].VariableType.GetMethod("Add"); //add method of target
                var src = playerGameModelVariables.First(x => x.Name == chartValues[i].Name); //property in source corrosponding to property target
                //double min = Double.MaxValue, max = Double.MinValue;
                while (em.MoveNext())
                {
                    add.Invoke(target, new object[] { src.GetValue(em.Current) }); //add to target value of source
                    //var value = Convert.ToDouble(src.GetValue(em.Current));
                    //if (value < min) min = value;
                    //if (value > max) max = value;
                }
                em.Reset();
            }
            //labels
            Labels = Source.Select(x => x.StartTime.ToString()).ToArray();
            AxisXMinValue = AxisXLowerValue = -1;
            AxisXMaxValue = AxisXUpperValue = Source.Count;
            //dims
            UpdateSeriesDimensions();
        }

        private void UpdateSeriesDimensions()
        {
            for (int i = 0; i < chartValues.Length; i++)
            {
                var enumValue = (Dimensions)Enum.Parse(typeof(Dimensions), chartValues[i].Name, true); //enum value assiciated with property of ViewModel
                var index = Series.FindIndex(x => x is ISeriesView sv && sv.Title == chartValues[i].Name); //index of seriesview corrosponding to the chartvalue property, otherwise -1
                var values = chartValues[i].GetValue(this) as IChartValues;
                if (index == -1) //add missing seriesview
                {
                    index = Series.Count;
                    Series.Add(SeriesViewFactory(values, chartValues[i].Name, i));
                }
                else //update values of seriesview
                {
                    Series[index].Values = values;
                }
                var visibility = (SeriesDimensions & enumValue) == enumValue ? Visibility.Visible : Visibility.Hidden;
                (Series[index] as Series).Visibility = visibility;
                AxisYCollection[index].Visibility = visibility;
            }
        }

        private static LineSeries SeriesViewFactory(IChartValues values, string name, int scaleIndex)
        {
            var series = new LineSeries()
            {
                Values = values,
                Title = name,
                ScalesYAt = scaleIndex,
                DataLabels = false,
                PointGeometrySize = 12,
            };
            return series;
        }
    }
}
