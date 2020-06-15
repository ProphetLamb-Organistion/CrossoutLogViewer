/* 
 * All credit to GrumpyDev (Steven Robbins)
 */
using CrossoutLogView.GUI.Core;
using CrossoutLogView.GUI.Events;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using System.Threading;

namespace CrossoutLogView.GUI.Services
{
    public static class ResourceManagerService
    {
        private static Dictionary<string, ResourceManager> _managers;

        public static event LocaleChangedEventHander LocaleChanged;
        private static void RaiseLocaleChanged(Locale newLocale)
        {
            var evt = LocaleChanged;

            if (evt != null)
                evt.Invoke(null, new LocaleChangedEventArgs(newLocale));
        }

        /// <summary>
        /// Current application locale
        /// </summary>
        public static Locale CurrentLocale { get; private set; }

        /// <summary>
        /// Initializes a new instance of the ResourceManager class.
        /// </summary>
        static ResourceManagerService()
        {
            _managers = new Dictionary<string, ResourceManager>();

            // Set to default culture
            ChangeLocale(CultureInfo.CurrentCulture.IetfLanguageTag);
        }

        /// <summary>
        /// Retreives a string resource with the given key from the given
        /// resource manager.
        /// 
        /// Will load the string relevant to the current culture.
        /// </summary>
        /// <param name="managerName">Name of the ResourceManager</param>
        /// <param name="resourceKey">Resource to lookup</param>
        /// <returns></returns>
        public static string GetResourceString(string managerName, string resourceKey)
        {
            string resource = String.Empty;

            if (_managers.TryGetValue(managerName, out var manager))
                resource = manager.GetString(resourceKey);

            return resource;
        }

        /// <summary>
        /// Changes the current locale
        /// </summary>
        /// <param name="newLocaleName">IETF locale name (e.g. en-US, en-GB)</param>
        public static void ChangeLocale(string newLocaleName)
        {
            CultureInfo newCultureInfo = new CultureInfo(newLocaleName);
            Thread.CurrentThread.CurrentCulture = newCultureInfo;
            Thread.CurrentThread.CurrentUICulture = newCultureInfo;

            Locale newLocale = new Locale() { Name = newLocaleName, RTL = newCultureInfo.TextInfo.IsRightToLeft };
            CurrentLocale = newLocale;

            RaiseLocaleChanged(newLocale);
        }

        /// <summary>
        /// Fires the LocaleChange event to reload bindings
        /// </summary>
        public static void Refresh()
        {
            ChangeLocale(CultureInfo.CurrentCulture.IetfLanguageTag);
        }

        /// <summary>
        /// Register a ResourceManager, does not fire a refresh
        /// </summary>
        /// <param name="managerName">Name to store the manager under, used with GetResourceString/UnregisterManager</param>
        /// <param name="manager">ResourceManager to store</param>
        public static void RegisterManager(string managerName, ResourceManager manager)
        {
            RegisterManager(managerName, manager, false);
        }

        /// <summary>
        /// Register a ResourceManager
        /// </summary>
        /// <param name="managerName">Name to store the manager under, used with GetResourceString/UnregisterManager</param>
        /// <param name="manager">ResourceManager to store</param>
        /// <param name="refresh">Whether to fire the LocaleChanged event to refresh bindings</param>
        public static void RegisterManager(string managerName, ResourceManager manager, bool refresh)
        {
            _managers.TryGetValue(managerName, out var _manager);

            if (_manager == null)
                _managers.Add(managerName, manager);

            if (refresh)
                Refresh();
        }

        /// <summary>
        /// Remove a ResourceManager
        /// </summary>
        /// <param name="name">Name of the manager to remove</param>
        public static void UnregisterManager(string name)
        {
            _managers.TryGetValue(name, out var _manager);

            if (_manager != null)
                _managers.Remove(name);
        }
    }
}
