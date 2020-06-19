﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace CrossoutLogView.GUI.Helpers
{
    public class BindingProxy : Freezable
    {
        public BindingProxy() { }
        public BindingProxy(object value)
            => Value = value;

        protected override Freezable CreateInstanceCore()
            => new BindingProxy();

        #region Value Property

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(object),
            typeof(BindingProxy),
            new FrameworkPropertyMetadata(default));

        public object Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        #endregion Value Property
    }
}
