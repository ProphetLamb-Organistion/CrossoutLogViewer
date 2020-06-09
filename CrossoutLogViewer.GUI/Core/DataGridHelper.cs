using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace CrossoutLogView.GUI.Core
{
    public static class DataGridHelper
    {
        /// <summary>
        /// Returns the source element of <see cref="RoutedEventArgs"/> from a <see cref="DataGrid"/>, if the type is  <see cref="DataGridColumnHeader"/> or <see cref="DataGridCell"/>.
        /// </summary>
        /// <param name="eventArgs">The <see cref="RoutedEventArgs"/> refering the <see cref="RoutedEventArgs.OriginalSource"/>.</param>
        /// <returns>The source <see cref="DataGridColumnHeader"/> or <see cref="DataGridCell"/>; otherwise <see cref="null"/>.</returns>
        public static object GetSourceCellElement(RoutedEventArgs eventArgs) => GetSourceCellElement(eventArgs.OriginalSource);

        /// <summary>
        /// Returns the source element of <see cref="RoutedEventArgs"/> from a <see cref="DataGrid"/>, if the type is  <see cref="DataGridColumnHeader"/> or <see cref="DataGridCell"/>>.
        /// </summary>
        /// <param name="originalSource">The <see cref="DependencyObject"/> provided by the <see cref="RoutedEventArgs"/>.</param>
        /// <returns>The source <see cref="DataGridColumnHeader"/> or <see cref="DataGridCell"/>; otherwise <see cref="null"/>.</returns>
        public static object GetSourceCellElement(object originalSource)
        {
            var dep = originalSource as DependencyObject;
            while (dep != null && !(dep is DataGridCell) && !(dep is DataGridColumnHeader)) dep = VisualTreeHelper.GetParent(dep);
            if (dep == null) return null;
            if (dep is DataGridColumnHeader dgch) return dgch;
            if (dep is DataGridCell dgc) return dgc;
            return null;
        }

        /// <summary>
        /// Retuns the source elements of <see cref="RoutedEventArgs"/> from a <see cref="DependencyObject"/>, if the type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="originalSource">The <see cref="DependencyObject"/> provided by the <see cref="RoutedEventArgs"/>.</param>
        /// <returns>The source elements of <see cref="RoutedEventArgs"/> from a <see cref="DependencyObject"/></returns>
        public static T GetSourceElement<T>(object originalSource) where T : DependencyObject
        {
            var type = typeof(T);
            var dep = originalSource as DependencyObject;
            while (dep != null && dep.GetType() != type) dep = VisualTreeHelper.GetParent(dep);
            if (dep == null) return null;
            return (T)dep;
        }
    }
}
