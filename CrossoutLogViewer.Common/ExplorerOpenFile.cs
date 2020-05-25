using System;
using System.Diagnostics;
using System.IO;

namespace CrossoutLogView.Common
{
    public static class ExplorerOpenFile
    {
        public static bool OpenFile(Uri fileUri)
        {
            return OpenFile(fileUri.AbsolutePath);
        }

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
            catch (Exception ex)
            {
                Logging.WriteLine(ex);
                return false;
            }
            return true;
        }
    }
}
