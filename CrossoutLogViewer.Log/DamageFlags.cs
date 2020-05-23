using CrossoutLogView.Common;

using System;

namespace CrossoutLogView.Log
{
    [Flags]
    public enum DamageFlag
    {
        None = 0,
        DMG_GENERIC = 1,
        DMG_DIRECT = 1 << 1,
        DMG_BLAST = 1 << 2,
        DMG_FLAME = 1 << 3,
        DMG_ENERGY = 1 << 4,
        DMG_COLLISION = 1 << 5,
        IGNORE_DAMAGE_SCALE = 1 << 6,
        CAR_PART = 1 << 7,
        HIGH_FIRE_RATE = 1 << 8,
        HUD_IMPORTANT = 1 << 9,
        CONTINUOUS = 1 << 10,
        CONTACT = 1 << 11,
        SUICIDE = 1 << 12,
        PIERCING = 1 << 13,
        HIGH_CAR_RESIST = 1 << 14,
        SUICIDE_DESPAWN = 1 << 15,
        All = ~0
    }

    public static class DamageFlagsUtility
    {
        public static DamageFlag FromString(string serialized)
        {
            var result = DamageFlag.None;
            foreach (var flag in serialized.Split(Strings.EnumDelimiter))
            {
                result |= (DamageFlag)Enum.Parse(typeof(DamageFlag), flag);
            }
            return result;
        }
    }
}
