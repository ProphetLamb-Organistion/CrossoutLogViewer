using System;

namespace CrossoutLogView.Database.Data
{
    public static class ImageProvider
    {
        public static Uri GetMapImageUri(string mapName)
        {
            return new Uri(Environment.CurrentDirectory + "//images//" + mapName.Trim() + ".jpg", UriKind.Absolute);
        }
    }
}
