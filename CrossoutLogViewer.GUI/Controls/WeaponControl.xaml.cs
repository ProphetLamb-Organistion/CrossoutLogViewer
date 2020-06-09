using CrossoutLogView.Database.Data;
using CrossoutLogView.GUI.Core;
using CrossoutLogView.GUI.Events;
using CrossoutLogView.GUI.Models;

using JetBrains.Annotations;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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
    /// Interaction logic for WeaponDataGrid.xaml
    /// </summary>
    public partial class WeaponControl
    {
        public event OpenModelViewerEventHandler OpenViewModelDoubleClick;
        public event SelectionChangedEventHandler SelectionChanged;
        public event WeaponFilterChangedEventHandler FilterChanged;

        public WeaponControl()
        {
            InitializeComponent();
            foreach (var column in DataGridWeapons.Columns)
            {
                column.CanUserSort = true;
                column.IsReadOnly = true;
            }
        }

        private WeaponFilter _weaponFilter;
        public WeaponFilter WeaponFilter
        {
            get => _weaponFilter;
            set
            {
                var oldValue = _weaponFilter;
                _weaponFilter = value;
                FilterChanged?.Invoke(this, new WeaponFilterChangedEventArgs(oldValue, _weaponFilter));
                var view = CollectionViewSource.GetDefaultView(DataGridWeapons.ItemsSource);
                view.Filter = WeaponFilter.Filter;
                view.Refresh();
            }
        }

        private void UserNameFilterTextChanged(object sender, TextChangedEventArgs e) => UserNameFilter = (sender as TextBox).Text;
        public string UserNameFilter
        {
            get => WeaponFilter.UserName;
            set => WeaponFilter = new WeaponFilter(WeaponFilter.WeaponName, value?.TrimStart());
        }
        public static readonly DependencyProperty UserNameFilterProperty = DependencyProperty.Register(nameof(UserNameFilter), typeof(string), typeof(WeaponControl),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        private void WeaponNameFilterTextChanged(object sender, TextChangedEventArgs e) => WeaponNameFilter = (sender as TextBox).Text;
        public string WeaponNameFilter
        {
            get => WeaponFilter.WeaponName;
            set => WeaponFilter = new WeaponFilter(value?.TrimStart(), WeaponFilter.UserName);
        }
        public static readonly DependencyProperty WeaponNameFilterProperty = DependencyProperty.Register(nameof(WeaponNameFilter), typeof(string), typeof(WeaponControl),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public IEnumerable<WeaponGlobalModel> ItemsSource
        {
            get => DataGridWeapons.ItemsSource as IEnumerable<WeaponGlobalModel>;
            set
            {
                if (value == null)
                    DataGridWeapons.ItemsSource = value;
                else
                {
                    DataGridWeapons.ItemsSource = value;
                    var view = (CollectionView)CollectionViewSource.GetDefaultView(DataGridWeapons.ItemsSource);
                    view.Filter = WeaponFilter.Filter;
                    view.Refresh();
                }
            }
        }

        public WeaponGlobalModel SelectedItem
        {
            get => DataGridWeapons.SelectedItem as WeaponGlobalModel;
            set
            {
                if (DataProvider.CompleteWeapon(value.Object))
                    value.UpdateCollections();
                GroupBoxOverview.DataContext = GroupBoxUsers.DataContext = DataGridWeapons.SelectedItem = value;
            }
        }

        private void WeaponOpenUserClick(object sender, RoutedEventArgs e)
        {
            object dataContext = null;
            var source = e.OriginalSource;
            if (source is Run r) source = r.Parent;
            if (DataGridHelper.GetSourceElement<ListBoxItem>(source) is ListBoxItem lbi) dataContext = lbi.DataContext;
            else if (sender is MenuItem mi && mi.CommandParameter is ContextMenu cm) dataContext = cm.DataContext;

            if (dataContext is WeaponUserListModel wul)
            {
                DataProvider.CompleteUser(wul.User);
                OpenViewModelDoubleClick?.Invoke(this, new OpenModelViewerEventArgs(new UserModel(wul.User), e));
            }
        }

        private void WeaponsSelectWeapon(object sender, SelectionChangedEventArgs e)
        {
            if (DataGridWeapons.SelectedItem is WeaponGlobalModel weapon)
            {
                SelectedItem = weapon;
                SelectionChanged?.Invoke(sender, e);
                e.Handled = true;
            }
        }
    }
}
