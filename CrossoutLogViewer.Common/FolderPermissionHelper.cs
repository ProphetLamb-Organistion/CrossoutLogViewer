using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Text;

namespace CrossoutLogView.Common
{
    public static class FolderPermissionHelper
    {
        public static bool CanRWXByDummy()
        {
            bool success = true;
            try
            {
                var dummyPath = @".\.dummy";
                // Try to create/write/read/delete a dummy file, to check for permissions
                File.CreateText(dummyPath).Dispose();
                File.WriteAllText(dummyPath, "dummy");
                File.ReadAllText(dummyPath);
                File.Delete(dummyPath);
            }
            catch (IOException) { success = false; }
            catch (UnauthorizedAccessException) { success = false; }
            return success;
        }

        public static void SetFolderPermissions()
        {
            Process.Start(new ProcessStartInfo
            {
                CreateNoWindow = false, // Hide CLI window
                FileName = @"cmd.exe",
                Verb = "runas", // Run with elevated rights
                Arguments = String.Concat("/noprofile /user:Administrator /C ", Strings.ScriptFolderPermissions, " \"",
                Environment.CurrentDirectory, "\"") // Absolute path to the current directory
            });
        }
    }
}
