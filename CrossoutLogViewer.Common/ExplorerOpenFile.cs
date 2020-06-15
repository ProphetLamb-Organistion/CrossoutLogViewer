using System;
using System.Diagnostics;
using System.IO;

namespace CrossoutLogView.Common
{
    public static class ExplorerOpenFile
    {
        /// <summary>
        /// Opens a file from a <see cref="Uri"/> using the windows explorer.
        /// </summary>
        /// <param name="fileUri">The uri to open.</param>
        /// <returns>True if the proccess was spawned successfully, otherwise false.</returns>
        public static bool OpenFile(Uri fileUri)
        {
            return OpenFile(fileUri.AbsolutePath);
        }

        /// <summary>
        /// Opens a file path in the windows explorer.
        /// </summary>
        /// <param name="filePath">The file path to open.</param>
        /// <returns>True if the proccess was spawned successfully, otherwise false.</returns>
        public static bool OpenFile(string filePath)
        {
            if (!File.Exists(filePath)) return false;
            try
            {
                using var opener = new Process();
                opener.StartInfo.FileName = "explorer";
                opener.StartInfo.Arguments = "\"" + Path.GetFullPath(filePath) + "\"";
                opener.Start();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
