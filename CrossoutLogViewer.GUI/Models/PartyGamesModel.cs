using CrossoutLogView.Common;
using CrossoutLogView.GUI.Core;
using CrossoutLogView.Statistics;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace CrossoutLogView.GUI.Models
{
    public class PartyGamesModel : StatDisplayViewModeBase
    {
        private ObservableCollection<UserModel> _users;
        private ObservableCollection<GameModel> _games;
        private UserModel _selectedUser;
        private int _won, _lost, _unfinished;
        private double _winRate;
        private bool _usersExpanded = false;

        public PartyGamesModel(ObservableCollection<UserModel> users, ObservableCollection<GameModel> games) : base()
        {
            Users = users;
            Games = games;
            UpdateCollectionsSafe();
            Users.CollectionChanged += (s, e) => UpdateCollectionsSafe();
            Games.CollectionChanged += (s, e) => UpdateCollectionsSafe();
        }

        public ObservableCollection<UserModel> Users { get => _users; set => Set(ref _users, value); }
        public ObservableCollection<GameModel> Games { get => _games; set => Set(ref _games, value); }
        public UserModel SelectedUser { get => _selectedUser; set => Set(ref _selectedUser, value); }
        public double Winrate { get => _winRate; set => Set(ref _winRate, value); }
        public int GamesWon { get => _won; set => Set(ref _won, value); }
        public int GamesLost { get => _lost; set => Set(ref _lost, value); }
        public int GamesUnfinished { get => _unfinished; set => Set(ref _unfinished, value); }
        public bool UsersExpanded { get => _usersExpanded; set => Set(ref _usersExpanded, value); }

        protected override void UpdateCollections()
        {
            Winrate = Users[0].Winrate;
            GamesWon = Users[0].GamesWon;
            GamesLost = Users[0].GamesLost;
            GamesUnfinished = Users[0].GamesUnfinished;
        }

        public static IEnumerable<PartyGamesModel> Parse(IEnumerable<GameModel> games)
        {
            if (games is null)
                throw new ArgumentNullException(nameof(games));
            var partyPlayers = new Dictionary<int, IEnumerable<int>>(4);
            var partyGames = new Dictionary<int, ICollection<GameModel>>(16);
            // Group games by squads
            foreach (var g in games)
            {
                // Add game to squads
                foreach (var pp in g.Players.GroupBy(x => x.Player.PartyID).Where(x => x.Key != 0))
                {
                    var userIds = pp.Select(x => x.UserID).OrderBy(x => x);
                    // Hash code from user ids
                    int hash = ArrayHashing.Checksum(userIds);
                    // The squad exists
                    if (partyPlayers.ContainsKey(hash))
                    {
                        // Add the game
                        partyGames[hash].Add(g);
                    }
                    else
                    {
                        // Create the squad
                        partyPlayers.Add(hash, userIds);
                        // Add the game
                        partyGames.Add(hash, new Collection<GameModel> { g });
                    }
                }
            }
            // Wrap each squad in PartyGamesModel
            foreach (var hash in partyPlayers.Keys)
            {
                var userIds = partyPlayers[hash];
                // Parse users from games
                var users = User.Parse(partyGames[hash].Select(x => x.Game)).Where(x => userIds.Contains(x.UserID));
                // Wrap in models
                yield return new PartyGamesModel(
                    new ObservableCollection<UserModel>(users.Select(x => new UserModel(x))), 
                    new ObservableCollection<GameModel>(partyGames[hash])
                );
            }
        }
    }
}
