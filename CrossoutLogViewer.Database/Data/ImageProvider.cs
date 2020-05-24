using System;
using System.Collections.Generic;
using System.Text;

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
