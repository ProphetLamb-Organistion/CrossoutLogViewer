using System;

namespace CrossoutLogView.Statistics
{
    public abstract class WeaponBase
    {
        public string Name;
        public double CriticalDamage;
        public double ArmorDamage;

        protected WeaponBase()
        {
            Name = String.Empty;
        }
    }
}
