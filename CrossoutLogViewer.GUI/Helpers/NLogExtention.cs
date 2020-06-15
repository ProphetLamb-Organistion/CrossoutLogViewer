using CrossoutLogView.GUI.Services;

using NLog;

using System;
using System.Collections.Generic;
using System.Text;

namespace CrossoutLogView.Common
{
    public static class NLogExtention
    {
        public static void TraceResource(this Logger logger, string resoureKey)
        {
            logger.Trace(ResourceManagerService.GetResourceString("LogResources", resoureKey));
        }
        public static void InfoResource(this Logger logger, string resoureKey)
        {
            logger.Info(ResourceManagerService.GetResourceString("LogResources", resoureKey));
        }
        public static void ErrorResource(this Logger logger, string resoureKey)
        {
            logger.Error(ResourceManagerService.GetResourceString("LogResources", resoureKey));
        }
    }
}
