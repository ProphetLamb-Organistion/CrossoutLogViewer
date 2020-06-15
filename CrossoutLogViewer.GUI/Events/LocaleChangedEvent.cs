/* 
 * All credit to GrumpyDev (Steven Robbins)
 */
using CrossoutLogView.GUI.Core;

using System;

namespace CrossoutLogView.GUI.Events
{
    public delegate void LocaleChangedEventHander(object sender, LocaleChangedEventArgs e);

    public class LocaleChangedEventArgs : EventArgs
    {
        public Locale NewLocale { get; set; }

        /// <summary>
        /// Initializes a new instance of the LocaleChangedEventArgs class.
        /// </summary>
        public LocaleChangedEventArgs(Locale newLocale)
        {
            NewLocale = newLocale;
        }
    }
}
