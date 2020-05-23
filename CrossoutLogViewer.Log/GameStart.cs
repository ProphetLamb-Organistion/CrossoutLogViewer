using CrossoutLogView.Common;

using System;

namespace CrossoutLogView.Log
{
    public class GameStart : ILogEntry
    {

        public GameStart(long timeStamp, string gameMode, string mapDisplayName)
        {
            TimeStamp = timeStamp;
            GameMode = gameMode;
            MapDisplayName = mapDisplayName;
        }

        public GameStart() { }

        public long TimeStamp { get; set; }
        public string GameMode;
        public string MapDisplayName;

        public static bool TryParse(in ReadOnlySpan<char> logLine, in DateTime logDate, out GameStart deserialized)
        {
            deserialized = default;
            if (logLine.Length < 30) return false;
            var parser = new Tokenizer();
            if (!parser.First(logLine, "===== Gameplay '")) return false;
            if (!parser.MoveNext(logLine, "' started, map '")) return false;
            var gameMode = parser.CurrentString;
            if (!parser.MoveNext(logLine, "'")) return false;
            var mapDisplayName = parser.CurrentString; var timeStamp = TimeConverter.FromString(logLine, logDate);
            deserialized = new GameStart(timeStamp, gameMode, mapDisplayName);
            return true;
        }
    }
}
