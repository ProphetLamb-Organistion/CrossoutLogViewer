using CrossoutLogView.Database.Data;
using CrossoutLogView.GUI.Core;
using CrossoutLogView.Statistics;

using System;
using System.Collections.Generic;
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
            Object = new Round();
            Parent = null;
            Kills = new List<KillModel>();
        }
        public RoundModel(GameModel parent, Round obj)
        {
            Parent = parent;
            Object = obj;
            obj.Kills.Sort((x, y) => x.Time.CompareTo(y.Time));
            Kills = obj.Kills.Select(x => new KillModel(this, x));
        }

        public override void UpdateCollections()
        {
            OnPropertyChanged(nameof(Kills));
        }

        public GameModel Parent { get; }

        public Round Object { get; }

        public IEnumerable<KillModel> Kills { get; }

        private bool _isExpanded = true;
        public bool IsExpanded { get => _isExpanded; set => Set(ref _isExpanded, value); }

        public string ListItemString
        {
            get
            {
                var sb = new StringBuilder();
                sb.Append("Round ");
                sb.Append(RoundNumber);
                sb.Append(CenterDotSeparator);
                sb.Append("from ");
                sb.Append((Object.Start - Parent.Start).ToString(@"mm\:ss"));
                sb.Append(" to ");
                sb.Append((Object.End - Parent.Start).ToString(@"mm\:ss"));
                sb.Append(CenterDotSeparator);
                sb.Append("duration ");
                sb.Append((Object.End - Object.Start).ToString(@"mm\:ss"));
                sb.Append(CenterDotSeparator);
                sb.Append(Object.Kills.Count);
                sb.Append(" kills");
                return sb.ToString();
            }
        }

        public int RoundNumber => Parent == null || Parent.Rounds == null ? 1 : Parent.Rounds.FindIndex(x => x.Start == Object.Start) + 1;

        public DateTime Start => Object.Start;

        public DateTime End => Object.End;

        public byte Winner => Object.Winner;

        public bool Won => Object.Winner == Parent.Players.First(x => x.UserID == Settings.Current.MyUserID).Team;

        public Brush Background => App.Current.Resources[Won ? "TeamWon" : "TeamLost"] as Brush;
    }
}
