using CrossoutLogView.Common;
using CrossoutLogView.Database.Connection;
using CrossoutLogView.Database.Data;
using CrossoutLogView.Database.Events;
using CrossoutLogView.Log;
using CrossoutLogView.Statistics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossoutLogView.Database.Collection
{
    public class StatisticsUploader : IDisposable
    {
        private List<ILogEntry> gameLog = new List<ILogEntry>();
        private List<Game> games = new List<Game>();
        private bool yield = false;

        public static InvalidateCachedDataEventHandler InvalidateCachedData;

        public StatisticsUploader(long beginTimeStamp, UploaderOperatingMode operatingMode = UploaderOperatingMode.Incremental)
        {
            LogEntryTimeStampLowerLimit = beginTimeStamp;
            OperatingMode = operatingMode;
        }

        public long LogEntryTimeStampLowerLimit { get; internal set; }

        public UploaderOperatingMode OperatingMode { get; }

        public void Commit()
        {
            if (OperatingMode != UploaderOperatingMode.Incremental) throw new InvalidOperationException();
            List<ILogEntry> delta;
            using (var logCon = new LogConnection())
            {
                delta = logCon.RequestAll(LogEntryTimeStampLowerLimit);
                logCon.Close();
            }
            if (delta.Count == 0) return;
            delta.Sort(new LogEntryTimeStampAscendingComparer());
            InternalCommit(delta);
        }

        public void Commit(IEnumerable<ILogEntry> sortedLogs)
        {
            if (OperatingMode != UploaderOperatingMode.Unchecked) throw new InvalidOperationException();
            InternalCommit(sortedLogs);
        }

        private void InternalCommit(IEnumerable<ILogEntry> delta)
        {
            LogEntryTimeStampLowerLimit = delta.Last().TimeStamp + 1; //move begin to after latest logentry

            bool hasFinish = false;
            foreach (var l in delta)
            {
                if (l is LevelLoad ll)
                {
                    if (yield)
                    {
                        if (!hasFinish) gameLog.Add(DummyGameFinish(gameLog[gameLog.Count - 1].TimeStamp + 1));
                        FinalizeGameLog();
                    }
                    yield = !IgnoreLevel(ll);
                    hasFinish = false;
                }
                if (yield)
                {
                    gameLog.Add(l);
                    if (l is GameFinish) hasFinish = true;
                }
            }
            if (games.Count != 0) //games were finished in the added logs
            {
                var users = User.ParseUsers(games);
                var weapons = WeaponGlobal.ParseWeapons(games);
                using (var statCon = new StatisticsConnection())
                {
                    using (var trans = statCon.BeginTransaction())
                    {
                        statCon.Insert(games);
                        trans.Commit();
                    }
                    using (var trans = statCon.BeginTransaction())
                    {
                        foreach (var u in users)
                        {
                            statCon.UpdateUser(u);
                        }
                        trans.Commit();
                    }
                    using (var trans = statCon.BeginTransaction())
                    {
                        foreach (var w in weapons)
                        {
                            statCon.UpdateWeaponGlobal(w);
                        }
                        trans.Commit();
                    }
                    statCon.Close();
                }
                if (OperatingMode == UploaderOperatingMode.Incremental)
                {
                    var userIDs = users.Select(x => x.UserID);
                    var weaponNames = weapons.Select(x => x.Name);
                    var maps = games.Select(x => x.Map.Name).Distinct();
                    //send event invalidating changed data
                    Logging.WriteLine<StatisticsUploader>(String.Concat("Invalidate existing data. ", maps.Count(), " different maps played. ", userIDs.Count(), " user changed. ", weapons.Count(), " weapons changed."));
                    InvalidateCachedData?.Invoke(this, new InvalidateCachedDataEventArgs(userIDs, weaponNames, maps));
                }
                games.Clear();
            }
        }

        public void Cleanup()
        {
            if (gameLog.Count != 0 && gameLog.Any(x => x is GameStart))
            {
                LogEntryTimeStampLowerLimit = gameLog[gameLog.Count - 1].TimeStamp + 1;
                gameLog.Add(DummyGameFinish(LogEntryTimeStampLowerLimit));
                FinalizeGameLog();
            }
            gameLog.Clear();
            yield = false;
        }

        public void Clear()
        {
            gameLog.Clear();
            games.Clear();
            yield = false;
        }

        private GameFinish DummyGameFinish(long dateTime)
        {
            return new GameFinish(dateTime, "unfinished", 0xff, "unfinished", (new TimeSpan(dateTime - gameLog[0].TimeStamp)).TotalSeconds);
        }

        private void FinalizeGameLog()
        {
            try
            {
                games.Add(Game.Parse(gameLog));
            }
            catch (PlayerNotFoundException ex)
            {
                Logging.WriteLine<StatisticsUploader>(ex);
            }
            catch (MatchingLogEntryNotFoundException ex)
            {
                Logging.WriteLine<StatisticsUploader>(ex);
            }
            catch (ArgumentNullException ex)
            {
                Logging.WriteLine<StatisticsUploader>(ex);
            }
            finally
            {
                gameLog.Clear();
            }
        }

        private static bool IgnoreLevel(LevelLoad levelLoad)
        {
            return levelLoad == null
                || String.IsNullOrEmpty(levelLoad.MapPathName)
                || String.Equals(Strings.LevelLoadNameTestDrive, levelLoad.MapPathName, StringComparison.InvariantCultureIgnoreCase)
                || String.Equals(Strings.LevelLoadNameMainMenu, levelLoad.MapPathName, StringComparison.InvariantCultureIgnoreCase);
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    gameLog = null;
                    games = null;
                }
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }

    internal class LogEntryTimeStampAscendingComparer : IComparer<ILogEntry>
    {
        int IComparer<ILogEntry>.Compare(ILogEntry x, ILogEntry y) => x.TimeStamp.CompareTo(y.TimeStamp);
    }
}
