/* 
 * All credit to GrumpyDev (Steven Robbins)
 */
using System;

namespace CrossoutLogView.GUI.Core
{
    public class Locale : ICloneable
    {
        /// <summary>
        /// Name of the locale
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///  Is the locale right to left
        /// </summary>
        public bool RTL { get; set; }

        object ICloneable.Clone() => Clone();
        public Locale Clone() => new Locale { Name = Name, RTL = RTL };
}
}
