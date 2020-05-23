using CrossoutLogView.Log;

using System;
using System.Collections.Generic;

namespace CrossoutLogView.Database.Collection
{
    public class LogCollector : IDisposable
    {
        public List<LevelLoad> LoadLevelList = new List<LevelLoad>();
        public List<TestDriveStart> StartTestDriveList = new List<TestDriveStart>();
        public List<TestDriveFinish> FinishTestDriveList = new List<TestDriveFinish>();
        public List<GameStart> StartGameList = new List<GameStart>();
        public List<GameFinish> FinishGameList = new List<GameFinish>();
        public List<GameRound> RoundGameList = new List<GameRound>();
        public List<ActiveBattleStart> StartActiveBattleList = new List<ActiveBattleStart>();
        public List<PlayerLoad> PlayerLoadList = new List<PlayerLoad>();
        public List<Damage> DamageList = new List<Damage>();
        public List<Killing> KillList = new List<Killing>();
        public List<KillAssist> KillAssistList = new List<KillAssist>();
        public List<Score> ScoreList = new List<Score>();
        public List<Decal> DecalList = new List<Decal>();

        public LogCollector(DateTime logDate)
        {
            LogDate = logDate;
        }

        public DateTime LogDate { get; }
        public DateTime First { get; private set; } = DateTime.MaxValue;
        public DateTime Last { get; private set; } = DateTime.MinValue;
        public ILogEntry Current { get; private set; }

        public void ClearAll()
        {
            LoadLevelList.Clear();
            StartTestDriveList.Clear();
            FinishTestDriveList.Clear();
            StartGameList.Clear();
            FinishGameList.Clear();
            RoundGameList.Clear();
            StartActiveBattleList.Clear();
            PlayerLoadList.Clear();
            DamageList.Clear();
            KillList.Clear();
            KillAssistList.Clear();
            ScoreList.Clear();
            DecalList.Clear();
        }

        public bool TryAdd(ReadOnlySpan<char> logLine)
        {
            if (LevelLoad.TryParse(logLine, LogDate, out var ll))
            {
                Current = ll;
                LoadLevelList.Add(ll);
            }
            else if (TestDriveStart.TryParse(logLine, LogDate, out var std))
            {
                Current = std;
                StartTestDriveList.Add(std);
            }
            else if (TestDriveFinish.TryParse(logLine, LogDate, out var ftd))
            {
                Current = ftd;
                FinishTestDriveList.Add(ftd);
            }
            else if (GameStart.TryParse(logLine, LogDate, out var sg))
            {
                Current = sg;
                StartGameList.Add(sg);
            }
            else if (GameFinish.TryParse(logLine, LogDate, out var fg))
            {
                Current = fg;
                FinishGameList.Add(fg);
            }
            else if (GameRound.TryParse(logLine, LogDate, out var rg))
            {
                Current = rg;
                RoundGameList.Add(rg);
            }
            else if (ActiveBattleStart.TryParse(logLine, LogDate, out var sab))
            {
                Current = sab;
                StartActiveBattleList.Add(sab);
            }
            else if (PlayerLoad.TryParse(logLine, LogDate, out var pl))
            {
                Current = pl;
                PlayerLoadList.Add(pl);
            }
            else if (Damage.TryParse(logLine, LogDate, out var dmg))
            {
                Current = dmg;
                DamageList.Add(dmg);
            }
            else if (Killing.TryParse(logLine, LogDate, out var kill))
            {
                Current = kill;
                KillList.Add(kill);
            }
            else if (KillAssist.TryParse(logLine, LogDate, out var ka))
            {
                Current = ka;
                KillAssistList.Add(ka);
            }
            else if (Score.TryParse(logLine, LogDate, out var sc))
            {
                Current = sc;
                ScoreList.Add(sc);
            }
            else if (Decal.TryParse(logLine, LogDate, out var dec))
            {
                Current = dec;
                DecalList.Add(dec);
            }
            else
            {
                Current = null;
                return false;
            }
            //update datetimes
            var currentDateTime = new DateTime(Current.TimeStamp);
            if (currentDateTime > Last) Last = currentDateTime;
            if (currentDateTime < First) First = currentDateTime;
            return true;
        }

        #region IDisposable Support
        private bool disposedValue = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }

                LoadLevelList = null;
                StartTestDriveList = null;
                FinishTestDriveList = null;
                StartGameList = null;
                FinishGameList = null;
                RoundGameList = null;
                StartActiveBattleList = null;
                PlayerLoadList = null;
                DamageList = null;
                KillList = null;
                KillAssistList = null;
                ScoreList = null;
                DecalList = null;

                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
