using CrossoutLogView.Common;

using System;
using System.Runtime.InteropServices;

namespace CrossoutLogView.Log
{
    [Guid("0747e051-7b1b-4324-9c94-a70c9e1ed72d")]
    public class Score : ILogEntry
    {
        public Score(long timeStamp, byte playerNumber, int scoreAmmount, string scoreReason)
        {
            TimeStamp = timeStamp;
            PlayerNumber = playerNumber;
            ScoreAmmount = scoreAmmount;
            ScoreReason = scoreReason;
        }

        public Score() { }

        public long TimeStamp { get; set; }
        public byte PlayerNumber;
        public int ScoreAmmount;
        public string ScoreReason;


        public static bool TryParse(in ReadOnlySpan<char> logLine, in DateTime logDate, out Score deserialized)
        {
            deserialized = default;
            if (logLine.Length < 30) return false;
            var parser = new Tokenizer();
            if (!parser.First(logLine, "Score:")) return false;
            if (!parser.First(logLine, "player:")) return false;
            if (!parser.MoveNext(logLine, ",")) return false;
            var playerNumber = parser.CurrentByte;
            if (!parser.First(logLine, "Got: ")) return false;
            if (!parser.MoveNext(logLine, ",")) return false;
            var scoreAmmount = parser.CurrentInt32;
            if (!parser.First(logLine, "reason: ")) return false;
            parser.End(logLine);
            var scoreReason = parser.CurrentString;
            var timeStamp = TimeConverter.FromString(logLine, logDate);
            deserialized = new Score(timeStamp, playerNumber, scoreAmmount, scoreReason);
            return true;
        }
    }
}
