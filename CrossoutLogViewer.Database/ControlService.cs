﻿using CrossoutLogView.Common;
using CrossoutLogView.Database.Collection;
using CrossoutLogView.Database.Connection;
using CrossoutLogView.Database.Data;

using NLog.Config;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Timers;

namespace CrossoutLogView.Database
{
    public class ControlService : ServiceBase
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly FileSystemWatcher fsWatcher;
        private readonly Timer logChangeTrackTimer;
        private readonly StatisticsUploader uploader;
        private LogConfig current = new LogConfig();

        public ControlService()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            logger.Trace("Initialize ControlService.");
            Settings.Update();

            //enure existence of all databases
            using (var c = new LogConnection()) { }
            using (var c = new StatisticsConnection()) { }

            CanStop = true;
            CanShutdown = true;
            CanPauseAndContinue = true;
            ServiceName = "CrossoutLogView.ControlService";

            fsWatcher = new FileSystemWatcher();
            fsWatcher.EnableRaisingEvents = false;
            fsWatcher.IncludeSubdirectories = true;
            fsWatcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.FileName;
            fsWatcher.Path = Settings.Current.LogRootPath;
            fsWatcher.Created += FileCreated;
            fsWatcher.Filter = "combat.log";

            logChangeTrackTimer = new Timer();
            logChangeTrackTimer.Enabled = false;
            logChangeTrackTimer.Interval = 2000.0;
            logChangeTrackTimer.Elapsed += FileChangedElapsed;

            uploader = new StatisticsUploader(Settings.Current.StatisticsParseDateTime);

            logger.Trace("Initialized ControlService.");
        }

        public void Start() => OnStart(null);

        public void DeleteDatabase()
        {
            fsWatcher.EnableRaisingEvents = false;
            uploader.Dispose();
            try
            {
                File.Delete(Strings.DataBaseLogPath);
                File.Delete(Strings.DataBaseStatisticsPath);
            }
            catch (IOException) { }
            Settings.Current.StatisticsParseDateTime = 0;
            Settings.Current.LogConfiguration = current = default;
            Settings.Current.MyName = Settings.Default.MyName;
            Settings.Current.MyUserID = Settings.Default.MyUserID;
        }

        protected override void OnStart(string[] args)
        {
            Settings.Update();
            var previous = Settings.Current.LogConfiguration;
            var updated = Settings.GetLatestLog();
            if (previous == default) //no previous log
            {
                current = updated;
                ParseLogDelta();
                ParseUnprocessedLogs();
                uploader.Cleanup();
            }
            else if (PathUtility.Equals(previous.Path, updated.Path)) //same log, different position
            {
                current = previous;
                ParseLogDelta();
            }
            else //different log
            {
                current = previous;
                ParseLogDelta();
                current = updated;
                ParseUnprocessedLogs();
                ParseLogDelta();
                uploader.Cleanup();
            }
            uploader.Commit();
            Settings.Current.StatisticsParseDateTime = uploader.LogEntryTimeStampLowerLimit;

            fsWatcher.EnableRaisingEvents = true;
            logChangeTrackTimer.Enabled = true;
        }

        protected override void OnStop()
        {
            fsWatcher.EnableRaisingEvents = false;
            logChangeTrackTimer.Enabled = false;
            Settings.Current.LogConfiguration = current;
        }

        private void FileCreated(object sender, FileSystemEventArgs e)
        {
            logChangeTrackTimer.Stop();
            try
            {
                var updated = Settings.GetLatestLog();
                if (!PathUtility.Equals(updated.Path, current.Path))
                {
                    logger.Info("New log found. Path: \"" + updated.Path + "\"");
                    AddLogMetadata(updated);
                    if (!current.Equals(default)) //finish previous log
                    {
                        ParseLogDelta();
                    }
                    current = updated;
                    ParseLogDelta();
                    uploader.Cleanup();
                    uploader.Commit();
                    Settings.Current.StatisticsParseDateTime = uploader.LogEntryTimeStampLowerLimit;
                }
            }
            catch (FileNotFoundException) { }
            logChangeTrackTimer.Start();
        }

        private void FileChangedElapsed(object sender, ElapsedEventArgs e)
        {
            if (current.Path is null) return;
            var newSize = PathUtility.GetFileSize(Path.Combine(current.Path, Strings.ComatLogName));
            if (current.FileSize < newSize) //the size of the combat.log increased
            {
                current.FileSize = newSize;
                ParseLogDelta();
                uploader.Commit();
                Settings.Current.StatisticsParseDateTime = uploader.LogEntryTimeStampLowerLimit;
            }
        }

        private void ParseLogDelta()
        {
            var filePath = Path.Combine(current.Path, Strings.ComatLogName);
            if (File.Exists(filePath))
            {
                using var logs = new LogUploader(filePath);
                logs.Reposition(current.Position);
                current.Position += logs.Parse();
                logs.Upload();
            }
            Settings.Current.LogConfiguration = current;
        }

        private void ParseUnprocessedLogs()
        {
            logger.Trace("Detect unprocessed logs.");
            var unprocessedLogs = GetUnprocessedLogPaths();
            if (unprocessedLogs.Count != 0)
            {
                logger.Info("Upload logs");
                Parallel.ForEach(unprocessedLogs, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount * 2 },
                    delegate (string dir)
                {
                    try
                    {
                        using var logUploader = new LogUploader(dir, UploaderOperatingMode.Unchecked);
                        using var statUploader = new StatisticsUploader(-1, UploaderOperatingMode.Unchecked);
                        logUploader.Parse();
                        logUploader.Upload();
                        statUploader.Commit(logUploader.Combined);
                    }
                    catch (FileNotFoundException) { }
                });
                using var logCon = new LogConnection();
                uploader.LogEntryTimeStampLowerLimit = logCon.RequestNewestLogEntryTimeStamp();
            }
            else logger.Info("Found none.");
            Settings.WriteInstance();
        }

        private static List<string> GetUnprocessedLogPaths()
        {
            var unprocessedLogs = new List<string>();
            using (var con = new LogConnection())
            {
                //compare logs in database to logs in directory, missing logs that contain a combat.log are unprocessed logs 
                var logsInDB = con.RequestMetadata().Select(x => x.Path);
                var logsInDir = Directory.GetDirectories(Settings.Current.LogRootPath);
                for (int i = 0; i < logsInDir.Length; i++)
                {
                    var dir = Path.Combine(logsInDir[i], Strings.ComatLogName);
                    if (File.Exists(dir) && !logsInDB.Any(x => PathUtility.Equals(x, dir)))
                    {
                        unprocessedLogs.Add(dir);
                        con.InsertMetadata(LogMetadata.Parse(dir));
                    }
                }
            }
            return unprocessedLogs;
        }

        private static void AddLogMetadata(LogConfig newConfig)
        {
            using var logCon = new LogConnection();
            logCon.InsertMetadata(new LogMetadata(PathUtility.GetDirectoryPath(newConfig.Path), newConfig.DateTime));
            logCon.Dispose();
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (fsWatcher != null) fsWatcher.Dispose();
                    if (logChangeTrackTimer != null) logChangeTrackTimer.Dispose();
                    if (uploader != null) uploader.Dispose();
                }
                disposedValue = true;
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}
