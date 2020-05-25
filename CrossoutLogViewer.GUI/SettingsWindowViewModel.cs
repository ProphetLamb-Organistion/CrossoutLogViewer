using ControlzEx.Theming;

using CrossoutLogView.Common;
using CrossoutLogView.Database.Data;
using CrossoutLogView.GUI.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace CrossoutLogView.GUI
{
    public class SettingsWindowViewModel : WindowViewModel
    {
        public SettingsWindowViewModel()
        {
            AccentColors = ThemeManager.Current.Themes
                .GroupBy(x => x.ColorScheme)
                .OrderBy(a => a.Key)
                .Select(a => new AccentColorMenuData { Name = a.Key, ColorBrush = a.First().ShowcaseBrush })
                .ToList();
            AccentColor = AccentColors.First(x => x.Name == App.Theme.ColorScheme);
            AppThemes = ThemeManager.Current.Themes
                .GroupBy(x => x.BaseColorScheme)
                .Select(x => x.First())
                .Select(a => new AppThemeMenuData { Name = a.BaseColorScheme, BorderColorBrush = a.Resources["MahApps.Brushes.ThemeForeground"] as Brush, ColorBrush = a.Resources["MahApps.Brushes.ThemeBackground"] as Brush })
                .ToList();
            AppTheme = AppThemes.First(x => x.Name == App.Theme.BaseColorScheme);
            var resources = App.Theme.LibraryThemes.First(x => x.Origin == "MahApps.Metro").Resources.MergedDictionaries.First();
            Colors = typeof(Colors).GetProperties(BindingFlags.Public | BindingFlags.Static)
                .Where(x => x.Name != "Transparent")
                .Select(x => new AccentColorMenuData { Name = x.Name, ColorBrush = new SolidColorBrush((Color)x.GetValue(null)) })
                .ToList();
        }

        public List<AccentColorMenuData> AccentColors { get; set; }

        public List<AppThemeMenuData> AppThemes { get; set; }

        public AccentColorMenuData AccentColor { get; set; }

        public List<AccentColorMenuData> Colors { get; set; }

        public AccentColorMenuData TotalDamage { get => AccentColorDataFromColor(Settings.Current.TotalDamage); set => ApplySettingsColor(value.Name); }

        public AccentColorMenuData CriticalDamage { get => AccentColorDataFromColor(Settings.Current.CriticalDamage); set => ApplySettingsColor(value.Name); }

        public AccentColorMenuData ArmorDamage { get => AccentColorDataFromColor(Settings.Current.ArmorDamage); set => ApplySettingsColor(value.Name); }

        public AccentColorMenuData Suicide { get => AccentColorDataFromColor(Settings.Current.Suicide); set => ApplySettingsColor(value.Name); }

        public AccentColorMenuData Despawn { get => AccentColorDataFromColor(Settings.Current.Despawn); set => ApplySettingsColor(value.Name); }

        public AccentColorMenuData TeamWon { get => AccentColorDataFromColor(Settings.Current.TeamWon); set => ApplySettingsColor(value.Name, true); }

        public AccentColorMenuData TeamLost { get => AccentColorDataFromColor(Settings.Current.TeamLost); set => ApplySettingsColor(value.Name, true); }

        public AppThemeMenuData AppTheme { get; set; }

        internal static void ApplyColors()
        {
            ApplySettingsColor(Settings.Current.TotalDamage, false, nameof(Settings.Current.TotalDamage));
            ApplySettingsColor(Settings.Current.CriticalDamage, false, nameof(Settings.Current.CriticalDamage));
            ApplySettingsColor(Settings.Current.ArmorDamage, false, nameof(Settings.Current.ArmorDamage));
            ApplySettingsColor(Settings.Current.Suicide, false, nameof(Settings.Current.Suicide));
            ApplySettingsColor(Settings.Current.Despawn, false, nameof(Settings.Current.Despawn));
            ApplySettingsColor(Settings.Current.TeamWon, true, nameof(Settings.Current.TeamWon));
            ApplySettingsColor(Settings.Current.TeamLost, true, nameof(Settings.Current.TeamLost));
        }

        internal void ResetColors()
        {
            ApplySettingsColor(Settings.Default.TotalDamage, false, nameof(Settings.Current.TotalDamage));
            OnPropertyChanged(nameof(TotalDamage));
            ApplySettingsColor(Settings.Default.CriticalDamage, false, nameof(Settings.Current.CriticalDamage));
            OnPropertyChanged(nameof(CriticalDamage));
            ApplySettingsColor(Settings.Default.ArmorDamage, false, nameof(Settings.Current.ArmorDamage));
            OnPropertyChanged(nameof(ArmorDamage));
            ApplySettingsColor(Settings.Default.Suicide, false, nameof(Settings.Current.Suicide));
            OnPropertyChanged(nameof(Suicide));
            ApplySettingsColor(Settings.Default.Despawn, false, nameof(Settings.Current.Despawn));
            OnPropertyChanged(nameof(Despawn));
            ApplySettingsColor(Settings.Default.TeamWon, true, nameof(Settings.Current.TeamWon));
            OnPropertyChanged(nameof(TeamWon));
            ApplySettingsColor(Settings.Default.TeamLost, true, nameof(Settings.Current.TeamLost));
            OnPropertyChanged(nameof(TeamLost));
        }

        private static void ApplySettingsColor(string color, bool setAlpha = false, [CallerMemberName] string resourceKey = "")
        {
            try
            {
                var c = (Color)ColorConverter.ConvertFromString(color);
                if (setAlpha) c.A = App.BaseColorScheme == "Light" ? App.LightThemeColorAlpha : App.DarkThemeColorAlpha;
                if (App.Current.Resources.Contains(resourceKey))
                {
                    App.Current.Resources[resourceKey] = new SolidColorBrush(c);
                    typeof(Settings).GetProperty(resourceKey).SetValue(Settings.Current, c.ToString());
                }
            }
            catch (FormatException ex)
            {
                Logging.WriteLine<App>(ex);
            }
        }

        private AccentColorMenuData AccentColorDataFromColor(string color)
        {
            var c = (Color)ColorConverter.ConvertFromString(color);
            c.A = 0xFF;
            var colors = Colors.Where(x => (x.ColorBrush as SolidColorBrush).Color == c);
            if (colors.Any()) return colors.First();
            return new AccentColorMenuData { Name = color, ColorBrush = new SolidColorBrush(c) };
        }
    }

    public class AccentColorMenuData
    {
        public string Name { get; set; }

        public Brush BorderColorBrush { get; set; }

        public Brush ColorBrush { get; set; }

        public AccentColorMenuData()
        {
            ChangeAccentCommand = new SimpleCommand(o => true, DoChangeTheme);
            BorderColorBrush = App.Theme.Resources["MahApps.Brushes.Gray5"] as Brush;
        }

        public ICommand ChangeAccentCommand { get; }

        protected virtual void DoChangeTheme(object sender)
        {
            ThemeManager.Current.ChangeThemeColorScheme(Application.Current, Name);
            App.Theme = ThemeManager.Current.DetectTheme();
            Settings.Current.ColorScheme = Name;
        }

        public static AccentColorMenuData GetCurrentAccent()
        {
            return new AccentColorMenuData { Name = App.Theme.ColorScheme, ColorBrush = App.Theme.ShowcaseBrush };
        }
    }

    public class AppThemeMenuData : AccentColorMenuData
    {
        protected override void DoChangeTheme(object sender)
        {
            ThemeManager.Current.ChangeThemeBaseColor(Application.Current, Name);
            App.Theme = ThemeManager.Current.DetectTheme();
            Settings.Current.BaseColorScheme = Name;
        }

        public static AppThemeMenuData GetCurrentTheme()
        {
            return new AppThemeMenuData { Name = App.Theme.BaseColorScheme, BorderColorBrush = App.Theme.Resources["MahApps.Brushes.ThemeForeground"] as Brush, ColorBrush = App.Theme.Resources["MahApps.Brushes.ThemeBackground"] as Brush };
        }
    }
}
