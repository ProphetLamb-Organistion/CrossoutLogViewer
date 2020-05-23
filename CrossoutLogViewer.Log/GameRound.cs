using CrossoutLogView.Common;

using System;

namespace CrossoutLogView.Log
{
    public class GameRound : ILogEntry
    {
        public GameRound(long timeStamp, byte roundNumber, string finishReason, byte team, string winReason, double roundDuration)
        {
            TimeStamp = timeStamp;
            RoundNumber = roundNumber;
            FinishReason = finishReason;
            Team = team;
            WinReason = winReason;
            RoundDuration = roundDuration;
        }

        public GameRound() { }

        public long TimeStamp { get; set; }
        public byte RoundNumber;
        public string FinishReason;
        public byte Team;
        public string WinReason;
        public double RoundDuration;

        public static bool TryParse(in ReadOnlySpan<char> logLine, in DateTime logDate, out GameRound deserialized)
        {
            deserialized = default;
            if (logLine.Length < 30) return false;
            var parser = new Tokenizer();
            if (!parser.First(logLine, "===== Best Of N round ")) return false;
            if (!parser.MoveNext(logLine, " finish, reason: ")) return false;
            var roundNumber = parser.CurrentByte;
            if (!parser.MoveNext(logLine, ", winner team ")) return false;
            var gameFinishReason = parser.CurrentString;
            if (!parser.MoveNext(logLine, ", win reason: ")) return false;
            var team = parser.CurrentByte;
            if (!parser.MoveNext(logLine, ", battle time: ")) return false;
            var winReason = parser.CurrentString;
            if (!parser.MoveNext(logLine, " sec")) return false;
            var roundDuration = parser.CurrentSingle;
            var timeStamp = TimeConverter.FromString(logLine, logDate);
            deserialized = new GameRound(timeStamp, roundNumber, gameFinishReason, team, winReason, roundDuration);
            return true;
        }
    }
}
