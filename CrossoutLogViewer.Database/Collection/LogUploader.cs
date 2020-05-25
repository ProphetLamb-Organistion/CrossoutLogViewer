using CrossoutLogView.Common;
using CrossoutLogView.Database.Connection;
using CrossoutLogView.Database.Events;
using CrossoutLogView.Log;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CrossoutLogView.Database.Collection
{
    public class LogUploader : IDisposable
    {
        private readonly LogCollector collector;
        private FileStream fileStream;
        private StreamReader streamReader;
        private long linePosition = 0;
        private readonly List<ILogEntry> _combined = new List<ILogEntry>();

        public static LogUploadEventEventHandler LogUploadEvent;

        public LogUploader(string logPath, UploaderOperatingMode operatingMode = UploaderOperatingMode.Incremental)
        {
            if (!PathUtility.TryParseCrossoutLogDirectoryName(PathUtility.GetDirectoryName(logPath), out var logDate))
                throw new ArgumentException("Directory name doesnot fullfill format requirement.", nameof(logPath));
            if (PathUtility.IsDirectory(logPath)) logPath = Path.Combine(logPath, Strings.ComatLogName);
            collector = new LogCollector(logDate);
            fileStream = new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            streamReader = new StreamReader(fileStream);
            OperatingMode = operatingMode;
        }

        public UploaderOperatingMode OperatingMode { get; }

        public IEnumerable<ILogEntry> Combined { get => OperatingMode == UploaderOperatingMode.Unchecked ? _combined : throw new InvalidOperationException(); }

        public void Reposition(long linePos)
        {
            if (linePosition == linePos) return;
            if (linePosition > linePos) //overshoot
            {
                var fs = new FileStream(fileStream.Name, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var sr = new StreamReader(fs);
                DisposeStreams();
                fileStream = fs;
                streamReader = sr;
                linePosition = 0;
            }
            //skip to desired line
            while (linePosition < linePos && streamReader.ReadLine() != null) linePosition++;
        }

        public int Parse()
        {
            int read = 0;
            string lineStr;
            while ((lineStr = streamReader.ReadLine()) != null)
            {
                if (collector.TryAdd(lineStr) && OperatingMode == UploaderOperatingMode.Unchecked) _combined.Add(collector.Current);
                read++;
            }
            linePosition += read;
            return read;
        }

        public void Upload()
        {
            using var logCon = new LogConnection();
            var oldTimeStamp = OperatingMode == UploaderOperatingMode.Incremental ? logCon.RequestNewestLogEntryTimeStamp() : 0;
            //upload collected logs
            Task.WaitAll(Task.Run(async delegate
            {
                using var trans = await logCon.BeginTransactionAsync();
                logCon.Insert(collector.DamageList);
                logCon.Insert(collector.DecalList);
                logCon.Insert(collector.FinishGameList);
                logCon.Insert(collector.FinishTestDriveList);
                logCon.Insert(collector.KillAssistList);
                logCon.Insert(collector.KillList);
                logCon.Insert(collector.LoadLevelList);
                logCon.Insert(collector.PlayerLoadList);
                logCon.Insert(collector.RoundGameList);
                logCon.Insert(collector.ScoreList);
                logCon.Insert(collector.StartActiveBattleList);
                logCon.Insert(collector.StartGameList);
                logCon.Insert(collector.StartTestDriveList);
                trans.Commit();
            }));
            if (OperatingMode == UploaderOperatingMode.Incremental)
                LogUploadEvent?.Invoke(this, new LogUploadEventArgs(new DateTime(oldTimeStamp), new DateTime(logCon.RequestNewestLogEntryTimeStamp())));
            logCon.Dispose();
            collector.ClearAll();
        }

        public string FilePath => (streamReader.BaseStream as FileStream).Name;

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    collector.Dispose();
                    DisposeStreams();
                }
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }

        private void DisposeStreams()
        {
            if (fileStream != null) fileStream.Dispose();
            if (streamReader != null) streamReader.Dispose();
        }
        #endregion

        private class TimeStampSorting : IComparer<ILogEntry>
        {
            int IComparer<ILogEntry>.Compare(ILogEntry x, ILogEntry y)
            {
                return x.TimeStamp.CompareTo(y.TimeStamp);
            }
        }
    }
}
