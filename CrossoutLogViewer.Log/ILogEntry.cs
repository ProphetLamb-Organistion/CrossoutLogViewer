
using System;
using System.Linq;

namespace CrossoutLogView.Log
{
    public interface ILogEntry
    {
        public long TimeStamp { get; set; }

        /// <summary>
        /// All types that implement this interface, excluding itslef.
        /// </summary>
        public static readonly Type[] Implementations = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(p => typeof(ILogEntry).IsAssignableFrom(p) && p.Name != nameof(ILogEntry)).ToArray();
    }
}
