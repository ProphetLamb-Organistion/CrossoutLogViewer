using CrossoutLogView.Common;

using System;

namespace CrossoutLogView.Log
{
    public class LevelLoad : ILogEntry
    {
        public LevelLoad(long timeStamp, short levelStart, string mapPathName)
        {
            TimeStamp = timeStamp;
            LevelStart = levelStart;
            MapPathName = mapPathName;
        }

        public LevelLoad() { }

        public long TimeStamp { get; set; }
        public short LevelStart;
        public string MapPathName;


        public static bool TryParse(in ReadOnlySpan<char> logLine, in DateTime logDate, out LevelLoad deserialized)
        {
            deserialized = default;
            if (logLine.Length < 30) return false;
            var parser = new Tokenizer();
            if (!parser.First(logLine, "====== starting level ")) return false;
            if (!parser.MoveNext(logLine, ": '")) return false;
            var levelStart = parser.CurrentInt16;
            if (!parser.MoveNext(logLine, "'")) return false;
            var mapPathName = parser.CurrentString;
            var timeStamp = TimeConverter.FromString(logLine, logDate);
            deserialized = new LevelLoad(timeStamp, levelStart, mapPathName);
            return true;
        }
    }
}
