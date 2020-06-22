using CrossoutLogView.GUI.Core;

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CrossoutLogView.GUI.Controls
{
    /// <summary>
    /// Interaction logic for DateSlider.xaml
    /// </summary>
    public partial class DateSlider : ILogging
    {
        private int _minimum = -7, _maximum = 7, value = 0;
        public event RoutedPropertyChangedEventHandler<int> ValueChanged;

        public DateSlider()
        {
            InitializeComponent();
            Refresh();
        }

        /// <summary>
        /// The value filling the middle slot of the <see cref="DateSlider"/>.
        /// </summary>
        public string Middle { get => GetValue(MiddleProperty) as string; protected set { SetValue(MiddlePropertyKey, value); } }
        public static readonly DependencyPropertyKey MiddlePropertyKey = DependencyProperty.RegisterReadOnly(nameof(Middle), typeof(string), typeof(DateSlider), new FrameworkPropertyMetadata(String.Empty, RefreshOnPropertyChanged));
        public static readonly DependencyProperty MiddleProperty = MiddlePropertyKey.DependencyProperty;

        /// <summary>
        /// The value filling the left slot of the <see cref="DateSlider"/>.
        /// </summary>
        public string Left { get => GetValue(LeftProperty) as string; protected set { SetValue(LeftPropertyKey, value); } }
        public static readonly DependencyPropertyKey LeftPropertyKey = DependencyProperty.RegisterReadOnly(nameof(Left), typeof(string), typeof(DateSlider), new FrameworkPropertyMetadata(String.Empty, RefreshOnPropertyChanged));
        public static readonly DependencyProperty LeftProperty = LeftPropertyKey.DependencyProperty;

        /// <summary>
        /// The value filling the right slot of the <see cref="DateSlider"/>.
        /// </summary>
        public string Right { get => GetValue(RightProperty) as string; protected set { SetValue(RightPropertyKey, value); } }
        public static readonly DependencyPropertyKey RightPropertyKey = DependencyProperty.RegisterReadOnly(nameof(Right), typeof(string), typeof(DateSlider), new FrameworkPropertyMetadata(String.Empty, RefreshOnPropertyChanged));
        public static readonly DependencyProperty RightProperty = RightPropertyKey.DependencyProperty;


        /// <summary>
        /// The value representing the upper bound of valid values.
        /// </summary>
        public int Maximum { get => (int)GetValue(MaximumProperty); set => SetValue(MaximumProperty, value); }
        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(nameof(Maximum), typeof(int), typeof(DateSlider), new PropertyMetadata(7, MaximumPropertyChanged));


        /// <summary>
        /// The value representing the lower bound of valid values.
        /// </summary>
        public int Minimum { get => (int)GetValue(MinimumProperty); set => SetValue(MinimumProperty, value); }
        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(nameof(Minimum), typeof(int), typeof(DateSlider), new PropertyMetadata(-7, MinimumPropertyChanged));


        /// <summary>
        /// The current value of the <see cref="DateSlider"/>.
        /// </summary>
        public int Value { get => (int)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(int), typeof(DateSlider), new PropertyMetadata(0, ValuePropertyChanged));


        private static void MaximumPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (!(obj is DateSlider ds))
                throw new ArgumentException("The parameter must be of the type DateSlider.", nameof(obj));
            if (!(e.NewValue is int newValue))
                newValue = (int)(e.OldValue ?? MaximumProperty.DefaultMetadata.DefaultValue);
            if (ds.Minimum > newValue)
                ds.Minimum = newValue;
            if (ds.Value > newValue)
                ds.Value = newValue;
            ds._maximum = newValue;
            ds.Refresh();
        }

        private static void MinimumPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (!(obj is DateSlider ds))
                throw new ArgumentException("The parameter must be of the type DateSlider.", nameof(obj));
            if (!(e.NewValue is int newValue))
                newValue = (int)(e.OldValue ?? MinimumProperty.DefaultMetadata.DefaultValue);
            if (ds.Maximum < newValue)
                ds.Maximum = newValue;
            if (ds.Value < newValue)
                ds.Value = newValue;
            ds._minimum = newValue;
            ds.Refresh();
        }

        private static void ValuePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (!(obj is DateSlider ds))
                throw new ArgumentException("The parameter must be of the type DateSlider.", nameof(obj));
            int oldValue = (int)(e.OldValue ?? ValueProperty.DefaultMetadata.DefaultValue);
            if (!(e.NewValue is int newValue))
                ds.Value = newValue = oldValue;
            if (newValue == oldValue)
                return;
            if (ds.Maximum < newValue)
                ds.Value = newValue = ds.Maximum;
            else if (ds.Minimum > newValue)
                ds.Value = newValue = ds.Minimum;
            ds.value = newValue;
            ds.Refresh();
            ds.ValueChanged?.Invoke(ds, new RoutedPropertyChangedEventArgs<int>(oldValue, newValue));
        }

        private static void RefreshOnPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (!(obj is DateSlider ds))
                throw new ArgumentException("The argument must be of the type DateSlider.", nameof(obj));
            ds.Refresh();
        }

        public void Refresh()
        {
            if (!Validate(value))
            {
                if (value < _minimum)
                    value = _minimum;
                else if (value > _maximum)
                    value = _maximum;
            }
            Middle = DisplayStringFromValue(value);

            int leftV = value - 1, rightv = value + 1;
            if (Validate(leftV))
                Left = DisplayStringFromValue(leftV);
            else
                Left = String.Empty;
            if (Validate(rightv))
                Right = DisplayStringFromValue(rightv);
            else
                Right = String.Empty;
        }

        private void NavigateUp(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is Label lb)) return;
            var pos = e.GetPosition(Grid);
            if (lb.Name == "LabelLeft")
            {
                Value--;
                e.Handled = true;
            }
            else if (lb.Name == "LabelRight")
            {
                Value++;
                e.Handled = true;
            }
        }

        private static string DisplayStringFromValue(int value)
        {
            return value switch
            {
                0 => App.GetControlResource("Date_Tdy"),
                1 => App.GetControlResource("Date_Tmrw"),
                -1 => App.GetControlResource("Date_Ystdy"),
                _ => value.ToString(CultureInfo.CurrentUICulture.NumberFormat),
            };
        }

        private bool Validate(int? value) => value != null && _minimum <= value && value <= _maximum;

        #region ILogging support
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        NLog.Logger ILogging.Logger { get; } = logger;
        #endregion
    }
}
