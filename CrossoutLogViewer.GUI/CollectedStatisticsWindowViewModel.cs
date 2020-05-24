using ControlzEx.Theming;

using CrossoutLogView.Common;
using CrossoutLogView.Database;
using CrossoutLogView.Database.Collection;
using CrossoutLogView.Database.Data;
using CrossoutLogView.Database.Events;
using CrossoutLogView.GUI.Core;
using CrossoutLogView.GUI.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace CrossoutLogView.GUI
{
    public sealed class CollectedStatisticsWindowViewModel : WindowViewModel
    {
        public static event InvalidateCachedDataEventHandler InvalidatedCachedData;

        public CollectedStatisticsWindowViewModel()
        {
            UpdateCollections();
            DataProvider.InvalidateCachedData += OnInvalidateCachedData;
            SettingsWindowViewModel.ApplyColors();
        }

        public List<PlayerGameCompositeModel> PlayerGameModels { get; set; }

        public List<WeaponGlobalModel> WeaponModels { get; set; }

        public List<UserModel> UserListModels { get; set; }

        public UserModel MeUser { get; set; }

        public List<MapModel> Maps { get; set; }

        private string _userName;
        public string UserNameFilter { get => _userName; set => Set(ref _userName, value?.TrimStart()); }

        public override void UpdateCollections()
        {
            MeUser = new UserModel(DataProvider.GetUser(Settings.Current.MyUserID));
            MeUser.Participations.Sort(new PlayerGameCompositeModelStartTimeDescending());
            PlayerGameModels = MeUser.Participations;
            WeaponModels = new List<WeaponGlobalModel>();
            foreach (var w in DataProvider.GetWeapons())
            {
                WeaponModels.Add(new WeaponGlobalModel(w));
            }
            WeaponModels.Sort(new WeaponGlobalModelTotalUsesDescending());
            UserListModels = new List<UserModel>();
            foreach (var u in DataProvider.GetUsers())
            {
                UserListModels.Add(new UserModel(u));
            }
            UserListModels.Sort(new UserModelParticipationCountDescending());
            Maps = new List<MapModel>();
            foreach (var map in DataProvider.GetMaps())
            {
                Maps.Add(new MapModel(map));
            }
            Maps.Sort(new MapModelGamesPlayedDecending());
        }

        private void OnInvalidateCachedData(object sender, InvalidateCachedDataEventArgs e)
        {
            if (e == null) return;
            var uid = MeUser.Object.UserID;
            MeUser = new UserModel(DataProvider.GetUser(uid));
            MeUser.Participations.Sort(new PlayerGameCompositeModelStartTimeDescending());
            PlayerGameModels = MeUser.Participations;
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
                var index = UserListModels.FindIndex(x => x.Object.UserID == userId);
                var user = new UserModel(DataProvider.GetUser(userId));
                if (index == -1) UserListModels.Add(user);
                else UserListModels[index] = user;
            }
            UserListModels.Sort(new UserModelParticipationCountDescending());
            foreach (var mapName in e.MapsPlayed)
            {
                var index = Maps.FindIndex(x => x.Name == mapName);
                var map = new MapModel(DataProvider.GetMap(mapName));
                if (index == -1) Maps.Add(map);
                else Maps[index] = map;
            }
            Maps.Sort(new MapModelGamesPlayedDecending());
            InvalidatedCachedData?.Invoke(sender, e);
        }
    }
}
