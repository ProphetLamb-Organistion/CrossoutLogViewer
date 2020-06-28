using CrossoutLogView.Common;

using Newtonsoft.Json;

using Octokit;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CrossoutLogView.Updater
{
    public abstract class UpdaterBase
    {
        /// <summary>
        /// Gets or sets the GitHubClient used to interact with GitHub.
        /// </summary>
        protected GitHubClient Client { get; set; }

        /// <summary>
        /// Gets or sets the WebClient used to download the content.
        /// </summary>
        protected WebClient WebClient { get; set; }

        public abstract Task Update();

        protected async Task<FileMetadata[]> GetRemoteMetadata(string filePath)
        {
            if (String.IsNullOrEmpty(filePath))
                throw new ArgumentException("Value cannot be null or empty.", nameof(filePath));
            // Get file bytes from github
            byte[] bytes = null;
            try
            {
                bytes = await Client.Repository.Content.GetRawContent(Strings.RepositoryOwner, Strings.RepositoryName, filePath);
            }
            catch (AggregateException)
            {
                Console.WriteLine("File not found");
            }
            if (bytes is null || bytes.Length == 0)
                return Array.Empty<FileMetadata>();
            // Decode bytes to string
            var serialized = Encoding.UTF8.GetString(bytes);
            if (String.IsNullOrEmpty(serialized))
                return Array.Empty<FileMetadata>();
            // Retrive Array object from string
            return JsonConvert.DeserializeObject<FileMetadata[]>(serialized);
        }

        protected static FileMetadata[] ComputeMetadataDelta(FileMetadata[] local, FileMetadata[] remote)
        {
            if (local is null)
                throw new ArgumentNullException(nameof(local));
            if (remote is null)
                throw new ArgumentNullException(nameof(remote));
            var delta = new List<FileMetadata>(remote.Length);
            for (int i = 0; i < remote.Length; i++)
            {
                // Excludes metadata.json file
                if (Strings.MetadataFile.Equals(remote[i].Name, StringComparison.OrdinalIgnoreCase))
                    continue;
                // Find local file corrosponding with remote file
                var localIndex = Array.FindIndex(local, x => remote[i].Name.Equals(x.Name, StringComparison.Ordinal));
                // Local file doesnt exist
                if (localIndex == -1)
                    delta.Add(remote[i]);
                // Checksums are differnet
                else if (!local[localIndex].Sha.Equals(remote[i].Sha, StringComparison.Ordinal))
                    delta.Add(remote[i]);
            }
            Console.WriteLine("Updating files:" + Environment.NewLine + String.Join(Environment.NewLine, delta.Select(x => x.Name)));
            return delta.ToArray();
        }

        /// <summary>
        /// Obtains all files at the specified <paramref name="path"/> in the repository.
        /// </summary>
        /// <typeparam name="T">The type the file is deserialized to.</typeparam>
        /// <param name="path">The path in the repository.</param>
        /// <param name="converter">The converter used to convert the requested objest to the desired type.</param>
        /// <param name="selector">The selector used to determine which files to include. If <see cref="null"/> all files are included.</param>
        /// <returns>All files at the specified <paramref name="path"/> in the repository.</returns>
        protected async IAsyncEnumerable<(string Name, T Value)> GetDirectoryContent<T>(string path, Func<RepositoryContent, T> converter, Predicate<RepositoryContent> selector = null)
        {
            if (Client is null)
                throw new NullReferenceException("Client cannot be null.");
            var repositoryContents = await Client.Repository.Content.GetAllContents(Strings.RepositoryOwner, Strings.RepositoryName, path);
            for (int i = 0; i < repositoryContents.Count; i++)
            {
                if (selector is null || selector(repositoryContents[i]))
                {
                    Console.WriteLine("Downloading: " + repositoryContents[i].Name);
                    T value = default;
                    try
                    {
                        value = converter(repositoryContents[i]);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(String.Concat("Message: ", ex.Message, Environment.NewLine, "StackTrace: ", ex.StackTrace, Environment.NewLine, "Source: ", ex.Source));
                    }
                    yield return (repositoryContents[i].Name, value);
                }
            }
        }

        /// <summary>
        /// Generates the metadata file from local files.
        /// </summary>
        /// <returns></returns>
        public async Task GenerateMetadata(string path)
        {
            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException(path);
            var local = FileMetadata.FromPaths(HashFunction, Directory.GetFiles(path)).Where(x => !Strings.MetadataFile.Equals(x.Name, StringComparison.InvariantCulture)).ToArray();
            await FileMetadataHelper.WriteJson(local, Path.Combine(path, Strings.MetadataFile));
        }

        /// <summary>
        /// Computes the base64 encoded hash representation of <see cref="byte[] "/> <paramref name="bytes"/> using <see cref="SHA1"/>. 
        /// If <paramref name="bytes"/> is null or empty, returns an empty string. 
        /// </summary>
        /// <param name="bytes">The byte[] to compute.</param>
        /// <returns>Returns a string representing the SHA1 hash of a byte[].</returns>
        public static string HashFunction(byte[] bytes)
        {
            if (bytes is null || bytes.Length == 0)
                return String.Empty;
            byte[] hash = null;
            using (var sha = SHA1.Create())
            {
                hash = sha.ComputeHash(bytes);
            }
            if (hash is null || hash.Length == 0)
                return String.Empty;
            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Returns wether the file needs to be updated, based on the local files provided.
        /// </summary>
        /// <param name="includedFiles">List of files incuded.</param>
        /// <param name="content">The GitHub content file.</param>
        /// <returns>True if the file needs to be updated; otherwise false.</returns>
        protected static bool UpdateSelector(FileMetadata[] includedFiles, RepositoryContent content)
        {
            if (content is null) return false;
            if (includedFiles is null) throw new ArgumentNullException(nameof(includedFiles));
            var index = Array.FindIndex(includedFiles, x => x.Name.Equals(content.Name, StringComparison.InvariantCulture));
            return index != -1; // Content isnt up to date locally
        }
    }
}
