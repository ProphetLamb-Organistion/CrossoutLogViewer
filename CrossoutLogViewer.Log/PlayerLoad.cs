using CrossoutLogView.Common;

using System;

namespace CrossoutLogView.Log
{
    public class PlayerLoad : ILogEntry
    {
        public PlayerLoad(long timeStamp, byte playerNumber, int userID, int partyID, string playerNickName, byte team, bool isBot, int uR, int matchMakerHash)
        {
            TimeStamp = timeStamp;
            PlayerNumber = playerNumber;
            UserID = userID;
            PartyID = partyID;
            PlayerNickName = playerNickName;
            Team = team;
            IsBot = isBot;
            UR = uR;
            MatchMakerHash = matchMakerHash;
        }

        public PlayerLoad() { }

        public long TimeStamp { get; set; }
        public byte PlayerNumber;
        public int UserID;
        public int PartyID;
        public string PlayerNickName;
        public byte Team;
        public bool IsBot;
        public int UR;
        public int MatchMakerHash;

        public static bool TryParse(in ReadOnlySpan<char> logLine, in DateTime logDate, out PlayerLoad deserialized)
        {
            deserialized = default;
            var parser = new Tokenizer();
            if (!parser.First(logLine, "player ")) return false;
            if (!parser.MoveNext(logLine, ", uid ")) return false;
            var playerNumber = parser.CurrentByte;
            if (!parser.MoveNext(logLine, ", party ")) return false;
            var userID = parser.CurrentInt32;
            if (!parser.MoveNext(logLine, ", nickname: ")) return false;
            var partyID = parser.CurrentInt32;
            if (!parser.MoveNext(logLine, ", team: ")) return false;
            var playerNickname = parser.CurrentString;
            if (!parser.MoveNext(logLine, ", bot: ")) return false;
            var team = parser.CurrentByte;
            if (!parser.MoveNext(logLine, ", ur:")) return false;
            var isBot = parser.CurrentInt32 != 0;
            if (!parser.MoveNext(logLine, ", mmHash: ")) return false;
            var ur = parser.CurrentInt32;
            parser.End(logLine);
            var mmHash = parser.CurrentHex;
            var timeStamp = TimeConverter.FromString(logLine, logDate);
            deserialized = new PlayerLoad(timeStamp, playerNumber, userID, partyID, playerNickname, team, isBot, ur, mmHash);
            return true;
        }
    }
}
