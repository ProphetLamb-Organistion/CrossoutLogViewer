using CrossoutLogView.Log;

using System;

namespace CrossoutLogView.Statistics
{
    public class Assist : IStatisticData
    {
        public string Assistant;
        public Weapon Weapon;
        public double Elapsed;
        public double TotalDamage;
        public DamageFlag DamageFlags;

        public Assist()
        {
            Assistant = String.Empty;
            Weapon = default;
            Elapsed = TotalDamage = 0.0;
            DamageFlags = DamageFlag.None;
        }

        public Assist(string assistant, Weapon weapon, double elapsed, double totalDamage, DamageFlag damageFlags)
        {
            Assistant = assistant;
            Weapon = weapon;
            Elapsed = elapsed;
            TotalDamage = totalDamage;
            DamageFlags = damageFlags;
        }

        public bool IsCriticalDamage => (DamageFlags & DamageFlag.HUD_IMPORTANT) == DamageFlag.HUD_IMPORTANT;

        public override string ToString() => String.Concat(nameof(Assist), " ", Assistant, " ", TotalDamage, " ", DamageFlags);
    }
}
