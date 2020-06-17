using CrossoutLogView.Common;

using Octokit;

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace CrossoutLogView.Updater
{
    public class ImageUpdater : UpdaterBase, IDisposable
    {
        public ImageUpdater()
        {
            if (!Directory.Exists(Strings.ImagePath))
                Directory.CreateDirectory(Strings.ImagePath);
            Client = new GitHubClient(new ProductHeaderValue(Strings.RepositoryName));
            WebClient = new WebClient();
        }

        /// <summary>
        /// Syncronizes local image files with the images present obtained from GitHub.
        /// </summary>
        public override async Task Update()
        {
            // Obtain remote metadata file
            var remote = GetRemoteMetadata(Path.Combine(Strings.RemoteImagePath, Strings.MetadataFile)); // Remote path: resources\images\metadata.json
            // Generate local file metadata
            var local = FileMetadata.FromPaths(HashFunction, Directory.GetFiles(Strings.ImagePath));
            // All files that need updating
            var metadata = ComputeMetadataDelta(local, await remote);
            // Iterate through images in metadata
            var imageEnu = GetDirectoryContent(Strings.RemoteImagePath, ImageConverter).GetAsyncEnumerator();
            while (await imageEnu.MoveNextAsync())
            {
                // Write file to the images folder
                using var fs = new FileStream(Path.Combine(Strings.ImagePath, imageEnu.Current.Name), System.IO.FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                imageEnu.Current.Value.Save(fs, ImageFormat.Jpeg);
            }
        }

        private Image ImageConverter(RepositoryContent content)
        {
            if (content.Name.Split('.')[1] != "jpg")
                return null;
            using var fs = WebClient.OpenRead(new Uri(content.DownloadUrl));
            return new Bitmap(fs);
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
