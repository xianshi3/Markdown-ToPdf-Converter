using Avalonia;
using Avalonia.Media;
using System;
using System.Collections.Generic;

namespace MarkdownToPdfConverter.Services
{
    public interface IThemeService
    {
        string CurrentTheme { get; }
        ThemeResources CurrentResources { get; }
        void SetTheme(string themeName);
        void ApplyToApplication();
        event Action? ThemeChanged;
        ThemeResources GetResources(string themeName);
    }

    public class ThemeService : IThemeService
    {
        public static ThemeService Instance { get; } = new();

        private string _currentTheme = "Dark";
        public event Action? ThemeChanged;

        private readonly Dictionary<string, ThemeResources> _themes = new()
        {
            ["Dark"] = new ThemeResources
            {
                Background = new SolidColorBrush(Color.Parse("#0D1117")),
                SurfaceBackground = new SolidColorBrush(Color.Parse("#161B22")),
                Foreground = new SolidColorBrush(Color.Parse("#E6EDF3")),
                SecondaryForeground = new SolidColorBrush(Color.Parse("#8B949E")),
                BorderBrush = new SolidColorBrush(Color.Parse("#30363D")),
                AccentBrush = new SolidColorBrush(Color.Parse("#58A6FF")),
                AccentHoverBrush = new SolidColorBrush(Color.Parse("#79C0FF")),
                TextBoxBackground = new SolidColorBrush(Color.Parse("#0D1117")),
                TextBoxForeground = new SolidColorBrush(Color.Parse("#E6EDF3")),
                ButtonBackground = new SolidColorBrush(Color.Parse("#21262D")),
                ButtonHoverBackground = new SolidColorBrush(Color.Parse("#30363D")),
                ButtonForeground = new SolidColorBrush(Color.Parse("#C9D1D9")),
                StatusBarBackground = new SolidColorBrush(Color.Parse("#161B22")),
                TabItemForeground = new SolidColorBrush(Color.Parse("#8B949E")),
                TabItemSelectedForeground = new SolidColorBrush(Color.Parse("#E6EDF3")),
                TabSelectedBorder = new SolidColorBrush(Color.Parse("#F78166")),
                SuccessAccent = new SolidColorBrush(Color.Parse("#238636")),
                SuccessAccentHover = new SolidColorBrush(Color.Parse("#2EA043")),
                ReadyDotColor = new SolidColorBrush(Color.Parse("#3FB950")),
                StatValueColor = new SolidColorBrush(Color.Parse("#58A6FF")),
                ScrollBarThumb = new SolidColorBrush(Color.Parse("#484F58")),
                ScrollBarThumbHover = new SolidColorBrush(Color.Parse("#6E7681")),
            },
            ["Light"] = new ThemeResources
            {
                Background = new SolidColorBrush(Color.Parse("#FFFFFF")),
                SurfaceBackground = new SolidColorBrush(Color.Parse("#F5F5F5")),
                Foreground = new SolidColorBrush(Color.Parse("#24292F")),
                SecondaryForeground = new SolidColorBrush(Color.Parse("#656D76")),
                BorderBrush = new SolidColorBrush(Color.Parse("#D0D7DE")),
                AccentBrush = new SolidColorBrush(Color.Parse("#0969DA")),
                AccentHoverBrush = new SolidColorBrush(Color.Parse("#0550AE")),
                TextBoxBackground = new SolidColorBrush(Color.Parse("#FFFFFF")),
                TextBoxForeground = new SolidColorBrush(Color.Parse("#24292F")),
                ButtonBackground = new SolidColorBrush(Color.Parse("#F6F8FA")),
                ButtonHoverBackground = new SolidColorBrush(Color.Parse("#EAEEF2")),
                ButtonForeground = new SolidColorBrush(Color.Parse("#24292F")),
                StatusBarBackground = new SolidColorBrush(Color.Parse("#F5F5F5")),
                TabItemForeground = new SolidColorBrush(Color.Parse("#656D76")),
                TabItemSelectedForeground = new SolidColorBrush(Color.Parse("#24292F")),
                TabSelectedBorder = new SolidColorBrush(Color.Parse("#FD8C73")),
                SuccessAccent = new SolidColorBrush(Color.Parse("#1F883D")),
                SuccessAccentHover = new SolidColorBrush(Color.Parse("#1A7F37")),
                ReadyDotColor = new SolidColorBrush(Color.Parse("#1F883D")),
                StatValueColor = new SolidColorBrush(Color.Parse("#0969DA")),
                ScrollBarThumb = new SolidColorBrush(Color.Parse("#C0C0C0")),
                ScrollBarThumbHover = new SolidColorBrush(Color.Parse("#A0A0A0")),
            },
            ["Gray"] = new ThemeResources
            {
                Background = new SolidColorBrush(Color.Parse("#2E2E2E")),
                SurfaceBackground = new SolidColorBrush(Color.Parse("#363636")),
                Foreground = new SolidColorBrush(Color.Parse("#E0E0E0")),
                SecondaryForeground = new SolidColorBrush(Color.Parse("#888888")),
                BorderBrush = new SolidColorBrush(Color.Parse("#444444")),
                AccentBrush = new SolidColorBrush(Color.Parse("#6C5CE7")),
                AccentHoverBrush = new SolidColorBrush(Color.Parse("#7C6CF7")),
                TextBoxBackground = new SolidColorBrush(Color.Parse("#333333")),
                TextBoxForeground = new SolidColorBrush(Color.Parse("#E0E0E0")),
                ButtonBackground = new SolidColorBrush(Color.Parse("#3A3A3A")),
                ButtonHoverBackground = new SolidColorBrush(Color.Parse("#444444")),
                ButtonForeground = new SolidColorBrush(Color.Parse("#C9D1D9")),
                StatusBarBackground = new SolidColorBrush(Color.Parse("#363636")),
                TabItemForeground = new SolidColorBrush(Color.Parse("#888888")),
                TabItemSelectedForeground = new SolidColorBrush(Color.Parse("#E0E0E0")),
                TabSelectedBorder = new SolidColorBrush(Color.Parse("#F78166")),
                SuccessAccent = new SolidColorBrush(Color.Parse("#238636")),
                SuccessAccentHover = new SolidColorBrush(Color.Parse("#2EA043")),
                ReadyDotColor = new SolidColorBrush(Color.Parse("#3FB950")),
                StatValueColor = new SolidColorBrush(Color.Parse("#6C5CE7")),
                ScrollBarThumb = new SolidColorBrush(Color.Parse("#555555")),
                ScrollBarThumbHover = new SolidColorBrush(Color.Parse("#666666")),
            }
        };

        public string CurrentTheme => _currentTheme;
        public ThemeResources CurrentResources => _themes[_currentTheme];

        public void SetTheme(string themeName)
        {
            if (_themes.ContainsKey(themeName) && _currentTheme != themeName)
            {
                _currentTheme = themeName;
                ApplyToApplication();
                ThemeChanged?.Invoke();
            }
        }

        public void ApplyToApplication()
        {
            var app = Application.Current;
            if (app == null) return;

            var r = CurrentResources;
            app.Resources["ThemeBackground"] = r.Background;
            app.Resources["ThemeSurfaceBackground"] = r.SurfaceBackground;
            app.Resources["ThemeForeground"] = r.Foreground;
            app.Resources["ThemeSecondaryForeground"] = r.SecondaryForeground;
            app.Resources["ThemeBorderBrush"] = r.BorderBrush;
            app.Resources["ThemeAccentBrush"] = r.AccentBrush;
            app.Resources["ThemeAccentHoverBrush"] = r.AccentHoverBrush;
            app.Resources["ThemeTextBoxBackground"] = r.TextBoxBackground;
            app.Resources["ThemeTextBoxForeground"] = r.TextBoxForeground;
            app.Resources["ThemeButtonBackground"] = r.ButtonBackground;
            app.Resources["ThemeButtonHoverBackground"] = r.ButtonHoverBackground;
            app.Resources["ThemeButtonForeground"] = r.ButtonForeground;
            app.Resources["ThemeStatusBarBackground"] = r.StatusBarBackground;
            app.Resources["ThemeTabItemForeground"] = r.TabItemForeground;
            app.Resources["ThemeTabItemSelectedForeground"] = r.TabItemSelectedForeground;
            app.Resources["ThemeTabSelectedBorder"] = r.TabSelectedBorder;
            app.Resources["ThemeSuccessAccent"] = r.SuccessAccent;
            app.Resources["ThemeSuccessAccentHover"] = r.SuccessAccentHover;
            app.Resources["ThemeReadyDotColor"] = r.ReadyDotColor;
            app.Resources["ThemeStatValueColor"] = r.StatValueColor;
            app.Resources["ThemeScrollBarThumb"] = r.ScrollBarThumb;
            app.Resources["ThemeScrollBarThumbHover"] = r.ScrollBarThumbHover;
        }

        public ThemeResources GetResources(string themeName) =>
            _themes.TryGetValue(themeName, out var resources) ? resources : _themes["Dark"];
    }

    public class ThemeResources
    {
        public IBrush Background { get; set; } = Brushes.Transparent;
        public IBrush SurfaceBackground { get; set; } = Brushes.Transparent;
        public IBrush Foreground { get; set; } = Brushes.White;
        public IBrush SecondaryForeground { get; set; } = Brushes.Gray;
        public IBrush BorderBrush { get; set; } = Brushes.Gray;
        public IBrush AccentBrush { get; set; } = Brushes.Blue;
        public IBrush AccentHoverBrush { get; set; } = Brushes.LightBlue;
        public IBrush TextBoxBackground { get; set; } = Brushes.Black;
        public IBrush TextBoxForeground { get; set; } = Brushes.White;
        public IBrush ButtonBackground { get; set; } = Brushes.DarkGray;
        public IBrush ButtonHoverBackground { get; set; } = Brushes.Gray;
        public IBrush ButtonForeground { get; set; } = Brushes.White;
        public IBrush StatusBarBackground { get; set; } = Brushes.DarkGray;
        public IBrush TabItemForeground { get; set; } = Brushes.Gray;
        public IBrush TabItemSelectedForeground { get; set; } = Brushes.White;
        public IBrush TabSelectedBorder { get; set; } = Brushes.Orange;
        public IBrush SuccessAccent { get; set; } = Brushes.Green;
        public IBrush SuccessAccentHover { get; set; } = Brushes.DarkGreen;
        public IBrush ReadyDotColor { get; set; } = Brushes.Green;
        public IBrush StatValueColor { get; set; } = Brushes.Blue;
        public IBrush ScrollBarThumb { get; set; } = Brushes.Gray;
        public IBrush ScrollBarThumbHover { get; set; } = Brushes.DarkGray;
    }
}
