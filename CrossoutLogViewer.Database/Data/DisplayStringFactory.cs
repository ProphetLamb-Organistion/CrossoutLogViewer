using CrossoutLogView.Common;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.IO;

namespace CrossoutLogView.Database.Data
{
    public static class DisplayStringFactory
    {
        private static Dictionary<string, string> assets = new Dictionary<string, string>();
        private static Dictionary<string, string> stripes = new Dictionary<string, string>();
        private static Dictionary<string, string> maps = new Dictionary<string, string>();

        private static readonly Dictionary<string, string> defaultAssets = new Dictionary<string, string>();

        private static readonly Dictionary<string, string> defaultStripes = new Dictionary<string, string>();

        private static readonly Dictionary<string, string> defaultMaps = new Dictionary<string, string>();

        private static readonly string[] assetNamePrefixes =
        {
            "CarPart_",
            "Wheel_",
            "Gun_",
            "Cabin_",
            "Structure_",
        };

        public static string AssetName(string name)
        {
            if (assets.Count == 0 && !ReadDictionary(ref assets, nameof(assets)))
            {
                assets = defaultAssets;
                WriteDictionary(assets, nameof(assets));
            }
            if (assets.TryGetValue(name, out var result)) return result;
            //default behaviour
            var trimmed = name.AsSpan();
            foreach (var prefix in assetNamePrefixes)
            {
                if (trimmed.StartsWith(prefix))
                    trimmed = trimmed.Slice(prefix.Length);
            }
            return trimmed.ToString();
        }

        public static string StripeName(string name)
        {
            if (stripes.Count == 0 && !ReadDictionary(ref stripes, nameof(stripes)))
            {
                stripes = defaultStripes;
                WriteDictionary(stripes, nameof(stripes));
            }
            if (stripes.TryGetValue(name, out var result)) return result;
            //default behaviour
            if (name.StartsWith("Pvp") || name.StartsWith("Pve")) return name[3..^0];
            else return name;
        }

        public static string MapName(string name)
        {
            if (maps.Count == 0 && !ReadDictionary(ref maps, nameof(maps)))
            {
                maps = defaultMaps;
                WriteDictionary(maps, nameof(maps));
            }
            if (maps.TryGetValue(name, out var result)) return result;
            return name;
        }

        private static bool ReadDictionary(ref Dictionary<string, string> dictionary, string name)
        {
            var filePath = Path.Combine(Strings.ConfigPath, name + ".json");
            if (File.Exists(filePath) && (dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(filePath))) != null) return true;
            return false;
        }

        private static void WriteDictionary(Dictionary<string, string> dictionary, string name)
        {
            using var sw = File.CreateText(Path.Combine(Strings.ConfigPath, name + ".json"));
            sw.WriteLine(JsonConvert.SerializeObject(dictionary, Formatting.Indented));
            sw.Flush();
        }
    }
}
