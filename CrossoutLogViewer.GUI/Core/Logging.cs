using System;

namespace CrossoutLogView.GUI.Core
{
    internal interface ILogging
    {
        internal NLog.Logger Logger { get; }
    }
}
