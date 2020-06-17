using CrossoutLogView.Common;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Media.Imaging;

namespace CrossoutLogView.GUI.Helpers
{
    internal class ImageHelper
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public enum FormatSize
        {
            Small_128,
            Medium_256,
            Original_992
        }

        private static ConcurrentDictionary<string, BitmapImage> imageCache = new ConcurrentDictionary<string, BitmapImage>();

        private static string MapFilePath(string mapName, FormatSize size)
        {
            if (String.IsNullOrEmpty(mapName)) return String.Empty;
            // Relative path to map image
            string filename = @".\images\" + mapName.Trim();
            // Append size modifer
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
            // Append file extention
            filename += ".jpg";
            return filename;
        }

        internal static BitmapImage GetMapImage(string mapName, FormatSize size = FormatSize.Medium_256)
        {
            if (String.IsNullOrEmpty(mapName))
            {
                logger.Warn("Map name is null or empty");
                return null;
            }
            // Try to get cached image
            if (imageCache.TryGetValue(mapName, out var img))
                return img;
            // Get image path
            var path = MapFilePath(mapName, size);
            // Check for invalid path
            if (String.IsNullOrEmpty(path) || !File.Exists(path))
            {
                logger.Error("Map image not found. Path: \"" + path + "\"");
                return null;
            }
            // Open filestream to image at path
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            // Initialize BitmapImage
            img = new BitmapImage();
            img.BeginInit();
            img.StreamSource = fs;
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.EndInit();
            // Add image to cache
            imageCache.Add(mapName, img);
            return img;
        }
    }
}
