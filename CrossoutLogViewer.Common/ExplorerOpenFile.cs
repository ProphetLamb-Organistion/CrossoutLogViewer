using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace CrossoutLogView.Common
{
    public static class ExplorerOpenFile
    {
        public static bool OpenFile(string filePath)
        {
            if (!File.Exists(filePath)) return false;
            using var opener = new Process();
            opener.StartInfo.FileName = "explorer";
            opener.StartInfo.Arguments = "\"" + Path.GetFullPath(filePath) + "\"";
            opener.Start();
            return true;
        }
    }
}
