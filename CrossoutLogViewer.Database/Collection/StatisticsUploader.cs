using CrossoutLogView.Common;
using CrossoutLogView.Database.Connection;
using CrossoutLogView.Database.Events;
using CrossoutLogView.Log;
using CrossoutLogView.Statistics;

using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace CrossoutLogView.Database.Collection
{
    public class StatisticsUploader : IDisposable
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

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
            if (delta == null) return;

            bool hasFinish = false;
            using (var en = delta.GetEnumerator())
            {
                while (en.MoveNext())
                {
                    if (en.Current.TimeStamp > LogEntryTimeStampLowerLimit) LogEntryTimeStampLowerLimit = en.Current.TimeStamp;
                    if (en.Current is LevelLoad ll)
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
                        gameLog.Add(en.Current);
                        if (en.Current is GameFinish) hasFinish = true;
                    }
                }
            }
            if (games.Count != 0) //games were finished in the added logs
            {
                var users = User.Parse(games);
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
                        for (int i = 0; i < users.Count; i++)
                        {
                            statCon.UpdateUser(users[i]);
                        }
                        trans.Commit();
                    }
                    using (var trans = statCon.BeginTransaction())
                    {
                        for (int i = 0; i < weapons.Count; i++)
                        {
                            statCon.UpdateWeaponGlobal(weapons[i]);
                        }
                        trans.Commit();
                    }
                }
                if (OperatingMode == UploaderOperatingMode.Incremental)
                {
                    var userIDs = users.Select(x => x.UserID);
                    var weaponNames = weapons.Select(x => x.Name);
                    var maps = games.Select(x => x.Map.Name).Distinct();
                    //send event invalidating changed data
                    logger.Info("Invalidate existing data. {0} different maps played. {1} user changed. {2} weapons changed.", maps.Count(), userIDs.Count(), weapons.Count());
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
            catch (PlayerNotFoundException) { }
            catch (MatchingLogEntryNotFoundException) { }
            catch (ArgumentNullException) { }
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
