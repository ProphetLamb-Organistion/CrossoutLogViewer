using System;
using System.IO;

namespace CrossoutLogView.Database.Data
{
    public static class ImageProvider
    {
        public enum FormatSize
        {
            Small_128,
            Medium_256,
            Original_992
        }

        public static Uri GetMapImageUri(string mapName, FormatSize size = FormatSize.Medium_256)
        {
            if (String.IsNullOrEmpty(mapName)) return null;
            string filename = mapName.Trim();
            switch (size)
            {
                case FormatSize.Small_128:
                    filename += "_128x128";
                    break;
                default:
                case FormatSize.Medium_256:
                    filename += "_256x256";
                    break;
                case FormatSize.Original_992:
                    break;
            }
            var uri = new Uri(Environment.CurrentDirectory + "//images//" + filename + ".jpg", UriKind.Absolute);
            return File.Exists(uri.AbsolutePath) ? uri : null;
        }
    }
}
