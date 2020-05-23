using System;
using System.Linq;

namespace CrossoutLogView.Statistics
{
    public interface IStatisticData
    {
        /// <summary>
        /// All types that implement this interface, excluding itslef.
        /// </summary>
        public static Type[] Implementations = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(p => typeof(IStatisticData).IsAssignableFrom(p) && p.Name != nameof(IStatisticData)).ToArray();
    }
}
