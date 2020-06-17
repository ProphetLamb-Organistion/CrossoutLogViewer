using CrossoutLogView.Common;

using Newtonsoft.Json;

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CrossoutLogView.Updater
{
    public class FileMetadata
    {
        public FileMetadata() { }

        public FileMetadata(string sha, FileInfo fileInfo) : this(sha, fileInfo.Name, fileInfo.Length) { }

        public FileMetadata(string sha, string name, long size)
        {
            if (sha is null)
                throw new ArgumentNullException(nameof(sha));
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("Value cannot be null or empty", nameof(name));
            Sha = sha;
            Name = name;
            Size = size;
        }

        public string Sha { get; set; }

        public string Name { get; set; }

        public long Size { get; set; }

        public static FileMetadata[] FromPaths(Func<byte[], string> hashFunction, params string[] filePaths)
        {
            if (hashFunction is null) throw new ArgumentNullException(nameof(hashFunction));
            if (filePaths is null) return Array.Empty<FileMetadata>();
            var metadata = new FileMetadata[filePaths.Length];
            Parallel.For(0, filePaths.Length, delegate (int i)
            {
                using var fs = new FileStream(filePaths[i], FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var br = new BinaryReader(fs);
                metadata[i] = new FileMetadata(hashFunction(br.ReadAllBytes()), new FileInfo(filePaths[i]));
            });
            return metadata;
        }
    }

    public static class FileMetadataHelper
    {
        public static async Task WriteJson(this FileMetadata[] metadata, string targetFilePath)
        {
            if (metadata is null)
                throw new ArgumentNullException(nameof(metadata));
            if (String.IsNullOrEmpty(targetFilePath))
                throw new ArgumentException("Value cannot be null or or empty.", nameof(targetFilePath));
            using var fs = new FileStream(targetFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            await metadata.WriteJson(fs);
            if (!(fs is null))
                await fs.DisposeAsync();
        }
        public static async Task WriteJson(this FileMetadata[] metadata, Stream stream)
        {
            if (metadata is null)
                throw new ArgumentNullException(nameof(metadata));
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));
            using var sw = new StreamWriter(stream);
            await sw.WriteLineAsync(JsonConvert.SerializeObject(metadata, Formatting.Indented));
            await sw.DisposeAsync();
        }

        public static async Task<FileMetadata[]> ReadJson(string soruceFilePath)
        {
            if (String.IsNullOrEmpty(soruceFilePath))
                throw new ArgumentException("Value cannot be null or or empty.", nameof(soruceFilePath));
            using var fs = new FileStream(soruceFilePath, System.IO.FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var metadata = await ReadJson(fs);
            if (!(fs is null))
                await fs.DisposeAsync();
            return metadata;
        }
        public static async Task<FileMetadata[]> ReadJson(Stream stream)
        {
            using var sr = new StreamReader(stream ?? throw new ArgumentNullException(nameof(stream)));
            return JsonConvert.DeserializeObject<FileMetadata[]>(await sr.ReadToEndAsync());
        }
    }
}
