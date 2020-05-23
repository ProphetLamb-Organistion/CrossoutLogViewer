
using System;

namespace CrossoutLogView.Database.Data
{
    public readonly struct LogMetadata : IEquatable<LogMetadata>
    {
        public readonly string Path;
        public readonly long DateTime;

        public LogMetadata(string path, long dateTime)
        {
            Path = path;
            DateTime = dateTime;
        }

        public static LogMetadata Parse(string path)
        {
            var norm = Common.PathUtility.NormalizePath(path);
            return new LogMetadata(norm, Common.PathUtility.ParseCrossoutLogDirectoryName(Common.PathUtility.GetDirectoryName(path)).Ticks);
        }

        public override bool Equals(object obj) => obj is LogMetadata metadata && Equals(metadata);
        public bool Equals(LogMetadata other) => Path == other.Path && DateTime == other.DateTime;
        public override int GetHashCode() => HashCode.Combine(Path, DateTime);

        public static bool operator ==(LogMetadata left, LogMetadata right) => left.Equals(right);
        public static bool operator !=(LogMetadata left, LogMetadata right) => !(left == right);
    }
}
