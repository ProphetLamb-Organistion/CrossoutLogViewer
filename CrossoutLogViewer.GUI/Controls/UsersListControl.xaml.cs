using CrossoutLogView.Common;
using CrossoutLogView.GUI.Events;
using CrossoutLogView.GUI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// <summary>
    /// Interaction logic for UsersListControl.xaml
    /// </summary>
    public partial class UsersListControl : UserControl
    {
        public event OpenModelViewerEventHandler OpenViewModel;
        private UsersListControlModel viewModel;

        public UsersListControl()
        {
            InitializeComponent();
            DataContext = viewModel = new UsersListControlModel();
            viewModel.PropertyChanged += OnPropertyChanged;
        }

        public ObservableCollection<UserModel> ItemsSource { get => GetValue(ItemsSourceProperty) as ObservableCollection<UserModel>; set => SetValue(ItemsSourceProperty, value); }
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(nameof(ItemsSource), typeof(ObservableCollection<UserModel>), typeof(UsersListControl), new PropertyMetadata(OnItemsSourcePropertyChanged));

        public UserModel SelectedItem { get => GetValue(SelectedItemProperty) as UserModel; set => SetValue(SelectedItemProperty, value); }
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(nameof(SelectedItem), typeof(UserModel), typeof(UsersListControl), new PropertyMetadata(OnSelectedItemPropertyChanged));
        
        public string FilterUserName { get => viewModel.FilterUserName; set => viewModel.FilterUserName = value; }

        private static void OnItemsSourcePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is UsersListControl ulc)
            {
                if (e.OldValue != null && e.OldValue is ObservableCollection<UserModel> oldValue)
                    oldValue.CollectionChanged -= ulc.ItemsSource_CollectionChanged;
                if (e.NewValue != null && e.NewValue is ObservableCollection<UserModel> newValue)
                {
                    newValue.Sort(new UserModelParticipationCountDescending());
                    newValue.CollectionChanged += ulc.ItemsSource_CollectionChanged;
                    var userListView = (CollectionView)CollectionViewSource.GetDefaultView(newValue);
                    userListView.Filter = ulc.UserListFilter;
                }
            }
        }

        private static void OnSelectedItemPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is UsersListControl ulc)
            {
                if (e.NewValue != null && e.NewValue is UserModel model)
                {
                    ulc.PlayerGamesChart.ItemsSource = model.Participations;
                }
            }
        }

        private void UserOpenUserDoubleClick(object sender, OpenModelViewerEventArgs e)
        {
            OpenViewModel?.Invoke(this, e);
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(UserListModel.FilterUserName):
                    CollectionViewSource.GetDefaultView(ItemsSource).Refresh();
                    CollectionViewSource.GetDefaultView(UserListViewUsers.ItemsSource).Refresh();
                    break;
                default:
                    return;
            }
        }

        private void ItemsSource_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(ItemsSource).Refresh();
            CollectionViewSource.GetDefaultView(UserListViewUsers.ItemsSource).Refresh();
        }

        private bool UserListFilter(object obj)
        {
            if (String.IsNullOrEmpty(viewModel.FilterUserName)) return true;
            if (!(obj is UserModel ul)) return false;
            foreach (var part in viewModel.FilterUserName.TrimEnd().Split(' ', '-', '_'))
            {
                if (!ul.Object.Name.Contains(part, StringComparison.InvariantCultureIgnoreCase)) return false;
            }
            return true;
        }
    }
}
