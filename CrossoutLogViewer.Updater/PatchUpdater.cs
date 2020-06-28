using CrossoutLogView.Common;

using Octokit;

using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace CrossoutLogView.Updater
{
    public class PatchUpdater : UpdaterBase, IDisposable
    {
        public PatchUpdater()
        {
            if (!Directory.Exists(Strings.PatchPath))
                Directory.CreateDirectory(Strings.PatchPath);
            Client = new GitHubClient(new ProductHeaderValue(Strings.RepositoryName));
            WebClient = new WebClient();
        }

        /// <summary>
        /// Syncronizes local config files with the files present obtained from GitHub.
        /// </summary>
        public override async Task Update()
        {
            // Obtain remote metadata file
            var remote = GetRemoteMetadata(Path.Combine(Strings.RemotePatchPath, Strings.MetadataFile));
            // Generate local file metadata
            var local = FileMetadata.FromPaths(HashFunction, Directory.GetFiles(Strings.PatchPath));
            // All files that need updating
            var metadata = ComputeMetadataDelta(local, await remote);
            // Iterate though files in metadata
            await foreach (var (Name, Value) in GetDirectoryContent(Strings.RemotePatchPath, ConfigConverter, x => UpdateSelector(metadata, x)))
            {
                // Write file to the config folder
                using var fs = new FileStream(Path.Combine(Strings.PatchPath, Name), System.IO.FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                using var sw = new StreamWriter(fs);
                await sw.WriteLineAsync(Value);
            }
        }

        private string ConfigConverter(RepositoryContent content)
        {
            if (content is null)
                return String.Empty;
            using var fs = WebClient.OpenRead(new Uri(content.DownloadUrl));
            using var sr = new StreamReader(fs);
            return sr.ReadToEnd();
        }

        #region IDisposable members 
        private bool disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {

                }
                Client = null;
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
        }
        #endregion
    }
}
