using System;
using System.IO;

namespace CrossoutLogView.Common
{
    public static class PathUtility
    {
        /// <summary>
        /// Returns <see cref="true"/> if the <paramref name="path"/> is a directory; otherwise <see cref="false"/>.
        /// </summary>
        /// <param name="path">The path of a file or directory.</param>
        /// <returns>Returns <see cref="true"/> if the <paramref name="path"/> is a directory; otherwise <see cref="false"/>.</returns>
        public static bool IsDirectory(string path)
        {
            var di = new DirectoryInfo(path);
            return di.Exists;
        }

        /// <summary>
        /// Normalizes a path by resolving URL encoding, removing tailing and unifing directoryseparators, and resolving the absolute path.
        /// </summary>
        /// <param name="path">The path of a file or directory.</param>
        /// <returns>The normalized path.</returns>
        public static string NormalizePath(string path)
        {
            return Path.GetFullPath(new Uri(path).LocalPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        /// <summary>
        /// Returns if <paramref name="path1"/> equals <paramref name="path2"/>.
        /// </summary>
        /// <param name="path1">First path of a file or directory.</param>
        /// <param name="path2">Second path of a file or directory.</param>
        /// <returns><see cref="true"/> if the paths are equal; otherwise <see cref="false"/>.</returns>
        public static bool Equals(string path1, string path2)
        {
            if (String.IsNullOrWhiteSpace(path1) || String.IsNullOrWhiteSpace(path2)) return false;
            return NormalizePath(path1).Equals(NormalizePath(path2), StringComparison.InvariantCulture);
        }

        /// <summary>
        /// Returns the name of the files parent directory, if the <paramref name="path"/> is a file; otherwise the name of the directory without a path.
        /// </summary>
        /// <param name="path">The path of a file or directory.</param>
        /// <returns>The name of the files parent directory, if the <paramref name="path"/> is a file; otherwise the name of the directory without a path.</returns>
        public static string GetDirectoryName(string path)
        {
            if (String.IsNullOrWhiteSpace(path)) return null;
            if (!IsDirectory(path)) return new FileInfo(path).Directory.Name;
            return new DirectoryInfo(path).Name;
        }

        /// <summary>
        /// Returns the name and the extention of the file, separated by a dot. If the <paramref name="path"/> is a directory instead of a file, return <see cref="null"/>.
        /// </summary>
        /// <param name="path">The path of a file or directory.</param>
        /// <returns>The name and the extention of the file, separated by a dot. If the <paramref name="path"/> is a directory instead of a file, return <see cref="null"/>.</returns>
        public static string GetFileNameAndExtention(string path)
        {
            if (!Path.HasExtension(path)) return null;
            var fi = new FileInfo(path);
            return fi.Name + '.' + fi.Extension;
        }

        /// <summary>
        /// Same as <see cref="Path.GetDirectoryName"/>.
        /// </summary>
        /// <param name="path">The path of a file or directory.</param>
        /// <returns></returns>
        public static string GetDirectoryPath(string path)
        {
            return Path.GetDirectoryName(NormalizePath(path));
        }

        /// <summary>
        /// Returns the size of a file in bytes.
        /// </summary>
        /// <param name="path">The path of a file.</param>
        /// <returns>The size (in bytes) of a file.</returns>
        public static long GetFileSize(string path)
        {
            var fi = new FileInfo(path);
            if (fi.Exists)
                return fi.Length;
            return -1;
        }

        /// <summary>
        /// Parses the <paramref name="directoryName"/> to <see cref="DateTime"/> using the format yyyy.MM.dd HH.mm.ss with local time.
        /// </summary>
        /// <param name="directoryName">Name of the directory.</param>
        /// <param name="dateTime">The <see cref="DateTime"/> derived from the <paramref name="directoryName"/>.</param>
        /// <returns><see cref="true"/> if the <paramref name="directoryName"/> conforms with the format; otherwise <see cref="false"/>.</returns>
        public static bool TryParseCrossoutLogDirectoryName(string directoryName, out DateTime dateTime)
        {
            return DateTime.TryParseExact(directoryName, "yyyy.MM.dd HH.mm.ss", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeLocal, out dateTime);
        }

        /// <summary>
        /// Parses the <paramref name="directoryName"/> to <see cref="DateTime"/> using the format yyyy.MM.dd HH.mm.ss with local time.
        /// </summary>
        /// <param name="directoryName">Name of the directory.</param>
        /// <returns>The DateTime representation of the directory name.</returns>
        public static DateTime ParseCrossoutLogDirectoryName(string directoryName)
        {
            return DateTime.ParseExact(directoryName, "yyyy.MM.dd HH.mm.ss", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeLocal);
        }
    }
}
