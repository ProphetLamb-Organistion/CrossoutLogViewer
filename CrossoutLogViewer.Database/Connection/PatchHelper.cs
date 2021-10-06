using CrossoutLogView.Common;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CrossoutLogView.Database.Connection
{
    public static class PatchHelper
    {
        public static async IAsyncEnumerable<string> EnumeratePatches(string database, Version minVersion)
        {
            if (File.Exists(Strings.PatchPath))
            {
                foreach (var file in Directory.EnumerateFiles(Strings.PatchPath).Where(path => !path.Contains(Strings.MetadataFile, StringComparison.InvariantCultureIgnoreCase)))
                {
                    if (VerifyPatch(file, database, minVersion))
                    {
                        using var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        using var sr = new StreamReader(fs);
                        yield return await sr.ReadToEndAsync();
                    }
                }
            }
            else
            {
                await System.Threading.Tasks.Task.Run(() => Directory.CreateDirectory(Strings.PatchPath));
            }
        }

        public static bool VerifyPatch(string filePath, string database, Version minVersion)
        {
            if (String.IsNullOrEmpty(filePath))
                throw new ArgumentException("Value cannot be null or empty.", nameof(filePath));
            if (String.IsNullOrEmpty(database))
                throw new ArgumentException("Value cannot be null or empty.", nameof(database));
            var parameters = Path.GetFileNameWithoutExtension(filePath).Split('_');
            if (parameters.Length != 3)
                throw new FormatException("Invalid file name format. Must contain three '_'. filePath = " + filePath);
            // Version
            Version version;
            try
            {
                version = Version.Parse(parameters[0]);
            }
            catch (FormatException ex)
            {
                throw new FormatException("Invalid file name format. Invalid version format. filePath = " + filePath, ex);
            }
            // Target database
            var targetDatabase = parameters[1].Trim();
            if (String.IsNullOrEmpty(targetDatabase))
                throw new FormatException("Invalid file name format. Database string cannot be null or whitespace. filePath = " + filePath);

            return minVersion < version
                && database.Equals(targetDatabase, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
