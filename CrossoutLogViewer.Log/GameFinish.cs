using CrossoutLogView.Common;

using System;

namespace CrossoutLogView.Log
{
    public class GameFinish : ILogEntry
    {

        public GameFinish(long timeStamp, string gameFinishReason, byte team, string winReason, double gameDuration)
        {
            TimeStamp = timeStamp;
            GameFinishReason = gameFinishReason;
            Team = team;
            WinReason = winReason;
            GameDuration = gameDuration;
        }

        public GameFinish() { }

        public long TimeStamp { get; set; }
        public string GameFinishReason;
        public byte Team;
        public string WinReason;
        public double GameDuration;

        public static bool TryParse(in ReadOnlySpan<char> logLine, in DateTime logDate, out GameFinish deserialized)
        {
            deserialized = default;
            if (logLine.Length < 30) return false;
            var parser = new Tokenizer();
            if (!parser.First(logLine, "===== Gameplay finish, reason: ")) return false;
            if (!parser.MoveNext(logLine, ", winner team ")) return false;
            var gameFinishReason = parser.CurrentString;
            if (!parser.MoveNext(logLine, ", win reason: ")) return false;
            var team = parser.CurrentByte;
            if (!parser.MoveNext(logLine, ", battle time: ")) return false;
            var winReason = parser.CurrentString;
            if (!parser.MoveNext(logLine, " sec")) return false;
            var gameDuration = parser.CurrentSingle;
            var timeStamp = TimeConverter.FromString(logLine, logDate);
            deserialized = new GameFinish(timeStamp, gameFinishReason, team, winReason, gameDuration);
            return true;
        }
    }
}
