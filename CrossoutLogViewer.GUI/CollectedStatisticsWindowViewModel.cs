using ControlzEx.Theming;

using CrossoutLogView.Common;
using CrossoutLogView.Database;
using CrossoutLogView.Database.Collection;
using CrossoutLogView.Database.Data;
using CrossoutLogView.Database.Events;
using CrossoutLogView.GUI.Core;
using CrossoutLogView.GUI.Models;
using CrossoutLogView.GUI.WindowsAuxilary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace CrossoutLogView.GUI
{
    public sealed class CollectedStatisticsWindowViewModel : WindowViewModel
    {
        public static event InvalidateCachedDataEventHandler InvalidatedCachedData;
        private Dispatcher dispatcher;

        public CollectedStatisticsWindowViewModel(Dispatcher dispatcher) : base()
        {
            this.dispatcher = dispatcher;
            UpdateCollections();
            DataProvider.InvalidateCachedData += OnInvalidateCachedData;
            SettingsWindowViewModel.ApplyColors();
        }

        public ObservableCollection<PlayerGameCompositeModel> PlayerGameModels { get; set; }

        public ObservableCollection<WeaponGlobalModel> WeaponModels { get; set; }

        public ObservableCollection<UserModel> UserModels { get; set; }

        public UserModel MeUser { get; set; }

        public ObservableCollection<MapModel> Maps { get; set; }

        public override void UpdateCollections()
        {
            dispatcher.Invoke(delegate
            {
                MeUser = new UserModel(DataProvider.GetUser(Settings.Current.MyUserID));
                MeUser.Participations.Sort(new PlayerGameCompositeModelStartTimeDescending());
                PlayerGameModels = MeUser.Participations;
                WeaponModels = new ObservableCollection<WeaponGlobalModel>(DataProvider.GetWeapons().Select(w => new WeaponGlobalModel(w)));
                WeaponModels.Sort(new WeaponGlobalModelTotalUsesDescending());
                UserModels = new ObservableCollection<UserModel>(DataProvider.GetUsers().Select(u => new UserModel(u)));
                UserModels.Sort(new UserModelParticipationCountDescending());
                Maps = new ObservableCollection<MapModel>(DataProvider.GetMaps().Select(m => new MapModel(m)));
                Maps.Sort(new MapModelGamesPlayedDecending());
            });
        }

        private void OnInvalidateCachedData(object sender, InvalidateCachedDataEventArgs e)
        {
            if (e == null) return;
            dispatcher.Invoke(delegate
            {
                var uid = MeUser.Object.UserID;
                MeUser = new UserModel(DataProvider.GetUser(uid));
                MeUser.Participations.Sort(new PlayerGameCompositeModelStartTimeDescending());
                foreach (var game in MeUser.Participations)
                {
                    if (!PlayerGameModels.Any(g => g.StartTime == game.StartTime))
                    {
                        PlayerGameModels.Add(game);
                    }
                }
                PlayerGameModels.Sort(new PlayerGameCompositeModelStartTimeDescending());
                foreach (var wName in e.WeaponsChanged)
                {
                    var index = WeaponModels.FindIndex(x => x.Object.Name == wName);
                    var weapon = new WeaponGlobalModel(DataProvider.GetWeapon(wName));
                    if (index == -1) WeaponModels.Add(weapon);
                    else WeaponModels[index] = weapon;
                }
                WeaponModels.Sort(new WeaponGlobalModelTotalUsesDescending());
                foreach (var userId in e.UsersChanged)
                {
                    var index = UserModels.FindIndex(x => x.Object.UserID == userId);
                    var user = new UserModel(DataProvider.GetUser(userId));
                    if (index == -1) UserModels.Add(user);
                    else UserModels[index] = user;
                }
                UserModels.Sort(new UserModelParticipationCountDescending());
                foreach (var mapName in e.MapsPlayed)
                {
                    var index = Maps.FindIndex(x => x.Name == mapName);
                    var map = new MapModel(DataProvider.GetMap(mapName));
                    if (index == -1) Maps.Add(map);
                    else Maps[index] = map;
                }
                Maps.Sort(new MapModelGamesPlayedDecending());
            });
            InvalidatedCachedData?.Invoke(sender, e);
        }
    }
}
