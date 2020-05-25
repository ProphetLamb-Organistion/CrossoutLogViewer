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

        private static readonly Dictionary<string, string> defaultAssets = new Dictionary<string, string>()
        {
            { "CarPart_AutoGuidedCourseGun_epic", "Cricket" },
            { "CarPart_Harvester_legend", "Harvester" },
            { "CarPart_Gun_Flamethrower_light", "Remedy" },
            { "CarPart_Gun_Flamethrower_frontal", "Firebug" },
            { "CarPart_Gun_Flamethrower_fixed", "Draco" },
            { "CarPart_ModuleAmmoBig_epic", "Expanded ammo pack" },
            { "CarPart_PowerGiverExplosive_legend", "Appollo" },
            { "Cabin_Pestilence", "Blight" },
            { "CarPart_Gun_Catapult", "Incinerator" },
            { "CarPart_Gun_Minigun_Legend", "Reaper" },
            { "CarPart_Quadrocopter_Syfy", "Annihilator" },
            { "CarPart_Gun_Syfy_Tesla", "Spark" },
            { "CarPart_Hover_rare", "Icarus VII" },
            { "CarPart_Hover_rare_bundle", "Icarus IV" },
            { "CarPart_Gun_GrenadeLauncher_Shotgun", "Retcher" },
            { "CarPart_Gun_Syfy_Plazma_Legend", "Helios" },
            { "CarPart_EngineMini_epic", "Hotred" },
            { "CarPart_MechaLeg", "ML 200" },
            { "CarPart_Gun_BigCannon_EX_Legend", "Tsunami" },
            { "CarPart_Gun_Minigun", "MG13 Equalizer" },
            { "CarPart_Gun_Syfy_FusionRifle_legend", "Assembler" },
            { "CarPart_HomingMissileLauncherBurstR_legend", "Hurricane" },
            { "CarPart_Gun_Shotgun_Relic", "Breaker" },
            { "CarPart_MechaWheelLeg", "Bigram" },
            { "CarPart_Gun_Harpoon", "Skinner" },
            { "Cabin_Death", "The Call" },
            { "CarPart_Stealth_Seeker_epic", "Verifier" },
            { "Cabin_Workers_epic", "Omnibox" },
            { "CarPart_Gun_ShotGun_Garbage_legend", "Nidhogg" },
            { "CarPart_Gun_ClassicCrossbow", "Spike-1" },
            { "CarPart_Gun_DoubleCrossbow", "Toadfish" },
            { "Chassis_Spider", "Steppespider" },
            { "CarPart_Gun_Machinegun_epic", "Spectre-1" },
            { "CarPart_TankTrackBig_epic", "Goliath" },
            { "CarPart_Gun_BigCannon_EX_Relic", "Typhoon" },
            { "CarPart_Gun_SniperCrossbow", "Scorpion" },
            { "CarPart_Gun_BigCannon_Free_legend", "Mammoth" },
            { "Cabin_War", "Echo" },
            { "Cabin_Lambo", "Torero" },
            { "CarPart_Gun_MineLauncher_Legend", "Fortune" },
            { "CarPart_PowerGiver_epic", "Gasgen" },
            { "CarPart_Gun_GrenadeLauncher_Auto", "Gl-55 Impulse" },
            { "CarPart_Quadrocopter_epic", "MD-3 Owl" },
            { "Cabin_Famine", "Howl" },
            { "CarPart_PowerGiver_legend", "Appollo" },
            { "AutoGuidedCourseGun_Epic2", "Locust" },
        };

        private static readonly Dictionary<string, string> defaultStripes = new Dictionary<string, string>()
        {

        };

        private static readonly Dictionary<string, string> defaultMaps = new Dictionary<string, string>()
        {
            { "powerplant", "Powerplant"},
            { "sand_crater", "Crater"},
            { "cityruin", "Old Town"},
            { "bridge", "Bridge"},
            { "factory", "Factory"},
            { "sand_valley", "Desert Valley" },
            { "rockcity_2bases", "Founders Canyon"},
            { "tower", "Nameless tower" },
            { "abandoned_ship", "Sand gulf" },
            { "riverport", "Ship graveyard" },
            { "iron_way_center", "\"Control-17\" station" },
            { "geopp", "Naukograd" },
            { "chemical_plant", "Chemical plant" },
            { "castle", "Fortress" },
            { "conflagration", "Ashen ring" },
            { "arizona_silo", "Broken arrow" },
            { "rockcity", "Rock City" },
            { "building_yard3", "Sector EX" },
            { "arizona_castle", "Wrath of Khan" },
            { "miners_way", "Cursed mines" },
            { "cemetery_highway", "Dead Highway" },
            { "iron_way", "Eastern Array" },
            { "lost_coast", "Lost coast" },
            { "port", "Terminal-45" },
            { "red_rocks_territory", "Blood Rocks" }
        };

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
            var filePath = Path.Combine(Strings.DataBaseRootPath, name + ".json");
            if (File.Exists(filePath) && (dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(filePath))) != null) return true;
            return false;
        }

        private static void WriteDictionary(Dictionary<string, string> dictionary, string name)
        {
            using var sw = File.CreateText(Path.Combine(Strings.DataBaseRootPath, name + ".json"));
            sw.WriteLine(JsonConvert.SerializeObject(dictionary, Formatting.Indented));
            sw.Flush();
        }
    }
}
