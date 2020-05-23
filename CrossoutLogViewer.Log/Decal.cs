using CrossoutLogView.Common;

using System;

namespace CrossoutLogView.Log
{
    public class Decal : ILogEntry
    {
        public Decal(long timeStamp, string stripeName, int awardAmmount, byte playerNumber)
        {
            TimeStamp = timeStamp;
            StripeName = stripeName;
            AwardAmmount = awardAmmount;
            PlayerNumber = playerNumber;
        }

        public Decal() { }

        public long TimeStamp { get; set; }
        public string StripeName;
        public int AwardAmmount;
        public byte PlayerNumber;

        public static bool TryParse(in ReadOnlySpan<char> logLine, in DateTime logDate, out Decal deserialized)
        {
            deserialized = default;
            if (logLine.Length < 30) return false;
            var parser = new Tokenizer();
            if (!parser.First(logLine, "Stripe '")) return false;
            if (!parser.MoveNext(logLine, "' value increased by ")) return false;
            var stripeName = parser.CurrentString;
            if (!parser.MoveNext(logLine, "for player ")) return false;
            var awardAmmount = parser.CurrentInt32;
            if (!parser.MoveNext(logLine, "[")) return false;
            var playerNumber = parser.CurrentByte;
            var timeStamp = TimeConverter.FromString(logLine, logDate);
            deserialized = new Decal(timeStamp, stripeName, awardAmmount, playerNumber);
            return true;
        }
    }
}
