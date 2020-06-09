using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Timers;

namespace CrossoutLogView.Common
{
    public static class Logging
    {
        private static FileStream stream;
        private static StreamWriter writer;
        private static Timer closeTimer;
        private static Dictionary<string, DateTime> bookmarks = new Dictionary<string, DateTime>();
        private static bool isOpen;
        private static string _filePath = Strings.DataBaseEventLogPath;

        static Logging()
        {
            if (!Directory.Exists(Strings.DataBaseRootPath)) Directory.CreateDirectory(Strings.DataBaseRootPath);
            closeTimer = new Timer();
            closeTimer.Interval = 500.0;
            closeTimer.Elapsed += (sender, e) => Close();
            closeTimer.Enabled = false;
            closeTimer.AutoReset = false;
            isOpen = false;
        }

        public static string FilePath
        {
            get => _filePath;
            set
            {
                Close();
                _filePath = value;
            }
        }

        /// <summary>
        /// If the file exceeds the <paramref name="maxFileSize"/> the file will be deleted.
        /// </summary>
        /// <param name="maxFileSize"></param>
        public static void TrimFile(long maxFileSize)
        {
            if (File.Exists(Strings.DataBaseEventLogPath) && PathUtility.GetFileSize(Strings.DataBaseEventLogPath) >= maxFileSize)
            {
                Close();
                File.Delete(_filePath);
            }
        }

        /// <summary>
        /// Writes the <see cref="Exception"/> to the current log file.
        /// </summary>
        /// <typeparam name="TClass">The type of the class calling.</typeparam>
        /// <param name="exception">The caught exception to write.</param>
        public static void WriteLine<TClass>(Exception exception)
        {
            WriteLine(exception, typeof(TClass).Name);
        }

        /// <summary>
        /// Writes the <see cref="Exception"/> to the current log file.
        /// </summary>
        /// <typeparam name="TClass">The type of the class calling.</typeparam>
        /// <param name="sqlException">The caught SQLiteException to write.</param>
        /// <param name="faultCommand">The command that triggered the SQLiteException.</param>
        public static void WriteLine<TClass>(Exception sqlException, string faultCommand)
        {
            WriteLine(sqlException, faultCommand, typeof(TClass).Name);
        }

        /// <summary>
        /// Writes the <paramref name="message"/> to the log. The sequence {TP} is replaced with the time elapsed from the last stored bookmark, if any.
        /// </summary>
        /// <typeparam name="TClass">The type of the class calling.</typeparam>
        /// <param name="message">The message.</param>
        /// <param name="setBookmark">If <paramref name="setBookmark"/> is <see cref="true"/>, then the <see cref="DateTime.Now"/> is stored with the <typeparamref name="TClass"/> for later use.</param>
        public static void WriteLine<TClass>(string message, bool setBookmark = false)
        {
            WriteLine(message, setBookmark, typeof(TClass).Name);
        }

        /// <summary>
        /// Writes the <see cref="Exception"/> to the current log file.
        /// </summary>
        /// <param name="exception">The caught exception to write.</param>
        /// <param name="callerName">The string representing the name of the caller.</param>
        public static void WriteLine(Exception exception, [CallerMemberName] string callerName = "")
        {
            WriteString(String.Concat(exception.GetType().Name, ": ", exception.Message, "\r\nSource: ", exception.Source, "\r\nStacktrace: ", exception.StackTrace, "\r\nInner exception: ", exception.InnerException), callerName, false);
        }

        /// <summary>
        /// Writes the <see cref="Exception"/> to the current log file.
        /// </summary>
        /// <param name="sqlException">The caught SQLiteException to write.</param>
        /// <param name="faultCommand">The command that triggered the SQLiteException.</param>
        /// <param name="callerName">The string representing the name of the caller.</param>
        public static void WriteLine(Exception sqlException, string faultCommand, [CallerMemberName] string callerName = "")
        {
            WriteString(String.Concat(sqlException.GetType().Name, ": ", sqlException.Message, "\r\nSource: ", sqlException.Source, "\r\nStacktrace: ", sqlException.StackTrace, "\r\nInner exception: ", sqlException.InnerException, "\r\nFault command: ", faultCommand), callerName, false);
        }

        /// <summary>
        /// Writes the <paramref name="message"/> to the log. The sequence {TP} is replaced with the time elapsed from the last stored bookmark, if any.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="setBookmark">If <paramref name="setBookmark"/> is <see cref="true"/>, then the <see cref="DateTime.Now"/> is stored with the <typeparamref name="TClass"/> for later use.</param>
        /// <param name="callerName">The string representing the name of the caller.</param>
        public static void WriteLine(string message, bool setBookmark = false, [CallerMemberName] string callerName = "")
        {
            if (setBookmark) SetBookmark(DateTime.Now, callerName);
            WriteString(message, callerName);
        }

        private static void SetBookmark(DateTime now, string callerName)
        {
            if (!bookmarks.TryAdd(callerName, now)) bookmarks[callerName] = now;
        }

        private static string ApplyBookmark(string message, string callerName)
        {
            if (bookmarks.TryGetValue(callerName, out var bookmarkDateTime))
                return message.Replace("{TP}", (DateTime.Now - bookmarkDateTime).TotalSeconds.ToString("0.##") + "sec");
            return message;
        }

        private static void WriteString(string message, string callerName, bool useBookmark = true)
        {
            Open();
            var sb = new StringBuilder();
            sb.Append(DateTime.Now.ToString(System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat));
            sb.Append(" | ");
            sb.Append(callerName);
            sb.Append(": ");
            var header = sb.ToString();
            var lines = (useBookmark ? ApplyBookmark(message, callerName) : message).Split(Environment.NewLine);
            if (lines.Length == 1)
            {
                sb.Append(lines[0]);
                writer.WriteLine(sb.ToString());
            }
            else
            {
                var spacer = new string(' ', header.Length);
                var lastLine = lines.Length - 1;
                sb.Append(lines[0]);
                writer.WriteLine(sb.ToString());
                for (int i = 1; i < lastLine; i++)
                {
                    writer.WriteLine(spacer + lines[i]);
                }
                writer.WriteLine(spacer + lines[lastLine]);
            }
            ScheduleClose();
        }

        private static void Open()
        {
            //stop closetimer
            closeTimer.Stop();
            if (isOpen) return;
            //initialize streams
            stream = new FileStream(_filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            writer = new StreamWriter(stream);
            isOpen = true;
        }

        private static void ScheduleClose()
        {
            //restart closeTimer
            if (closeTimer.Enabled)
                closeTimer.Stop();
            closeTimer.Start();
        }

        private static void Close()
        {
            //dispose streams
            if (writer != null) writer.Dispose();
            if (stream != null) stream.Dispose();
            closeTimer.Stop();
            isOpen = false;
        }
    }
}
