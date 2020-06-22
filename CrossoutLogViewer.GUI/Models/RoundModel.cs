using CrossoutLogView.Database.Data;
using CrossoutLogView.GUI.Core;
using CrossoutLogView.Statistics;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Media;

using static CrossoutLogView.Common.Strings;

namespace CrossoutLogView.GUI.Models
{
    public sealed class RoundModel : ViewModelBase
    {
        public RoundModel()
        {
            Round = new Round();
            Game = null;
            Kills = new List<KillModel>();
        }
        public RoundModel(GameModel parent, Round obj)
        {
            Game = parent ?? throw new ArgumentNullException(nameof(parent));
            Round = obj ?? throw new ArgumentNullException(nameof(obj));
            obj.Kills.Sort((x, y) => x.Time.CompareTo(y.Time));
            Kills = obj.Kills.Select(x => new KillModel(this, x));
        }

        public GameModel Game { get; }

        public Round Round { get; }

        public IEnumerable<KillModel> Kills { get; }

        private bool _isExpanded = true;
        public bool IsExpanded { get => _isExpanded; set => Set(ref _isExpanded, value); }

        public int RoundNumber => Game == null || Game.Rounds == null ? 1 : Game.Rounds.FindIndex(x => x.Start == Round.Start) + 1;

        public DateTime Start => Round.Start;

        public DateTime End => Round.End;

        public byte Winner => Round.Winner;

        public bool Won => Round.Winner == Game.Players.FirstOrDefault(x => x.UserID == Settings.Current.MyUserID).Team;
    }
}
