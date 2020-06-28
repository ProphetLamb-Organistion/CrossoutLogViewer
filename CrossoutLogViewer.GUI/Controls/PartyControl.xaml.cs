using ControlzEx.Standard;

using CrossoutLogView.Common;
using CrossoutLogView.GUI.Events;
using CrossoutLogView.GUI.Models;
using CrossoutLogView.Statistics;

using NLog.Targets;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xaml;

namespace CrossoutLogView.GUI.Controls
{
    /// <summary>
    /// Interaction logic for PartyControl.xaml
    /// </summary>
    public partial class PartyControl : UserControl
    {
        private BackgroundWorker updatePartiesWorker = new BackgroundWorker();

        public event OpenModelViewerEventHandler OpenViewModel;
        public event ValueChangedEventHandler<UserModel> SelectedUserChanged;

        public PartyControl()
        {
            InitializeComponent(); 
            InitializeWorkers();
        }

        public ObservableCollection<GameModel> ItemsSource { get => GetValue(ItemsSourceProperty) as ObservableCollection<GameModel>; set => SetValue(ItemsSourceProperty, value); }
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(nameof(ItemsSource), typeof(ObservableCollection<GameModel>), typeof(PartyControl), new PropertyMetadata(OnItemsSourcePropertyChanged));

        public ImmutableList<PartyGamesModel> Parties { get => GetValue(PartiesProperty) as ImmutableList<PartyGamesModel>; set => SetValue(PartiesPropertyKey, value); }
        protected static readonly DependencyPropertyKey PartiesPropertyKey = DependencyProperty.RegisterReadOnly(nameof(Parties), typeof(ImmutableList<PartyGamesModel>), typeof(PartyControl), new PropertyMetadata());
        public static readonly DependencyProperty PartiesProperty = PartiesPropertyKey.DependencyProperty;

        private void UpdateParties(object sender, NotifyCollectionChangedEventArgs e) => UpdateParties();
        public void UpdateParties()
        {
            if (!(ItemsSource is null))
                updatePartiesWorker.RunWorkerAsync();
        }

        private static void OnItemsSourcePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is PartyControl cntr)
            {
                if (e.OldValue is ObservableCollection<GameModel> oldValue)
                    // Unsubscribe from oldValue
                    oldValue.CollectionChanged -= cntr.UpdateParties;
                if (e.NewValue is ObservableCollection<GameModel> newValue)
                {
                    // Generate paries
                    cntr.UpdateParties();
                    // Subscribe to newValue
                    cntr.ItemsSource.CollectionChanged += cntr.UpdateParties; ;
                }
            }
        }

        private void InitializeWorkers()
        {
            updatePartiesWorker.DoWork += async delegate (object sender, DoWorkEventArgs e)
            {
                // Capture ItemsSource in MTAThread
                IEnumerable<GameModel> games = await Dispatcher.InvokeAsync(() => ItemsSource);
                // Parse parties from games
                var parties = await Task.Run(PartyGamesModel.Parse(games).OrderByDescending(x => x.Games.Count).ToImmutableList);
                // Register event handlers to update chart on latest selected item changed
                foreach (var party in parties)
                {
                    party.PropertyChanged += Party_PropertyChanged;
                }
                // Appy the parties in STAThread
                await Dispatcher.InvokeAsync(new Action(delegate
                {
                    if (parties.Count != 0)
                        // Expand first expander, assume its my group
                        parties[0].UsersExpanded = true;
                    Parties = parties;
                }));
            };
        }

        private void Party_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is PartyGamesModel partyGames)
            {
                if (e.PropertyName == nameof(PartyGamesModel.SelectedUser))
                {
                    SelectedUserChanged?.Invoke(this, new ValueChangedEventArgs<UserModel>(null, partyGames.SelectedUser));
                }
                else if (e.PropertyName == nameof(PartyGamesModel.UsersExpanded) && partyGames.UsersExpanded == true)
                {
                    // Collapse all expanders except the one that opened
                    foreach (var party in Parties.Where(x => !Object.ReferenceEquals(partyGames, x)))
                    {
                        party.UsersExpanded = false;
                    }
                    // Select first user
                    partyGames.SelectedUser = partyGames.Users[0];
                    SelectedUserChanged?.Invoke(this, new ValueChangedEventArgs<UserModel>(null, partyGames.SelectedUser));
                }
            }
        }

        private void Button_PartyGames_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is PartyGamesModel model)
            {

            }
        }

        private void Button_PartyUsers_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is PartyGamesModel model)
            {
                OpenViewModel?.Invoke(this, new OpenModelViewerEventArgs(new UserListModel(model.Users), e));
            }
        }

        private void UserDateGrid_OpenViewModel(object sender, OpenModelViewerEventArgs e)
        {
            OpenViewModel?.Invoke(this, new OpenModelViewerEventArgs(e.ViewModel));
        }

        private void UserDateGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var args = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
            args.RoutedEvent = ScrollViewer.MouseWheelEvent;
            (Content as ScrollViewer).RaiseEvent(args);
        }
    }

    public class PartyUsersConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(string) || targetType == typeof(object))
            {
                if (value is PartyGamesModel model)
                {
                    return String.Join(", ", model.Users.Select(x => x.Name));
                }
            }
            throw new NotSupportedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
