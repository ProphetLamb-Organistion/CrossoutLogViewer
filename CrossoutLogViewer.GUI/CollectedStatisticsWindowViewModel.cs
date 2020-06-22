using ControlzEx.Theming;

using CrossoutLogView.Common;
using CrossoutLogView.Database.Data;
using CrossoutLogView.Database.Events;
using CrossoutLogView.GUI.Core;
using CrossoutLogView.GUI.Models;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace CrossoutLogView.GUI
{
    public sealed class CollectedStatisticsWindowViewModel : WindowViewModelBase
    {
        private ObservableCollection<PlayerGameModel> _playerGameModels;
        private ObservableCollection<WeaponGlobalModel> _weaponModel;
        private ObservableCollection<UserModel> _userModels;
        private UserModel _meUser;
        private ObservableCollection<MapModel> _maps;

        public static event InvalidateCachedDataEventHandler InvalidatedCachedData;

        public CollectedStatisticsWindowViewModel() : base()
        {
            Database.Collection.StatisticsUploader.InvalidateCachedData += OnInvalidateCachedData;
        }

        public CollectedStatisticsWindowViewModel(Dispatcher windowDispatcher) : this()
        {
            WindowDispatcher = windowDispatcher;
        }

        public ObservableCollection<PlayerGameModel> PlayerGameModels { get => _playerGameModels; set => Set(ref _playerGameModels, value); }

        public ObservableCollection<WeaponGlobalModel> WeaponModels { get => _weaponModel; set => Set(ref _weaponModel, value); }

        public ObservableCollection<UserModel> UserModels { get => _userModels; set => Set(ref _userModels, value); }

        public UserModel MeUser { get => _meUser; set => Set(ref _meUser, value); }

        public ObservableCollection<MapModel> Maps { get => _maps; set => Set(ref _maps, value); }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            SettingsWindowViewModel.ApplyColors();
        }

        protected override void UpdateCollections()
        {
            UserModel meUser = null;
            ObservableCollection<WeaponGlobalModel> weapons = null;
            ObservableCollection<UserModel> users = null;
            ObservableCollection<MapModel> maps = null;
            Task.WaitAll(
            Task.Run(delegate
            {
                meUser = new UserModel(DataProvider.GetUser(Settings.Current.MyUserID));
                meUser.Participations.Sort(new PlayerGameModelStartTimeDescending());
            }),
            Task.Run(delegate
            {
                weapons = new ObservableCollection<WeaponGlobalModel>(DataProvider.GetWeapons().Select(w => new WeaponGlobalModel(w)));
                weapons.Sort(new WeaponGlobalModelTotalUsesDescending());
            }),
            Task.Run(delegate
            {
                users = new ObservableCollection<UserModel>(DataProvider.GetUsers().Select(u => new UserModel(u)));
                users.Sort(new UserModelParticipationCountDescending());
            }),
            Task.Run(delegate
            {
                maps = new ObservableCollection<MapModel>(DataProvider.GetMaps().Select(m => new MapModel(m)));
                maps.Sort(new MapModelGamesPlayedDecending());
            }));
            WindowDispatcher.Invoke(delegate
            {
                MeUser = meUser;
                PlayerGameModels = MeUser.Participations;
                WeaponModels = weapons;
                UserModels = users;
                Maps = maps;
            });
        }

        private void OnInvalidateCachedData(object sender, InvalidateCachedDataEventArgs e)
        {
            if (e == null) return;
            WindowDispatcher.Invoke(delegate
            {
                var uid = MeUser.User.UserID;
                MeUser = new UserModel(DataProvider.GetUser(uid));
                MeUser.Participations.Sort(new PlayerGameModelStartTimeDescending());
                PlayerGameModels = MeUser.Participations;
                foreach (var wName in e.WeaponsChanged)
                {
                    var index = WeaponModels.FindIndex(x => x.Weapon.Name == wName);
                    var weapon = new WeaponGlobalModel(DataProvider.GetWeapon(wName));
                    if (index == -1) WeaponModels.Add(weapon);
                    else WeaponModels[index] = weapon;
                }
                WeaponModels.Sort(new WeaponGlobalModelTotalUsesDescending());
                foreach (var userId in e.UsersChanged)
                {
                    var index = UserModels.FindIndex(x => x.User.UserID == userId);
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
