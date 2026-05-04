using Avalonia;
using Avalonia.Media;
using System;
using System.Collections.Generic;

namespace MarkdownToPdfConverter.Services
{
    public interface IThemeService
    {
        string CurrentTheme { get; }
        void SetTheme(string themeName);
        event Action? ThemeChanged;
        ThemeResources GetResources(string themeName);
    }

    public class ThemeService : IThemeService
    {
        private string _currentTheme = "Dark";
        public event Action? ThemeChanged;

        private readonly Dictionary<string, ThemeResources> _themes = new()
        {
            ["Dark"] = new ThemeResources
            {
                Background = Color.Parse("#1E1E1E"),
                Foreground = Color.Parse("#D0D0D0"),
                Border = Color.Parse("#444444"),
                TextBoxBackground = Color.Parse("#333333"),
                TextBoxForeground = Color.Parse("#FFFFFF")
            },
            ["Light"] = new ThemeResources
            {
                Background = Color.Parse("#FFFFFF"),
                Foreground = Color.Parse("#303030"),
                Border = Color.Parse("#CCCCCC"),
                TextBoxBackground = Color.Parse("#FFFFFF"),
                TextBoxForeground = Color.Parse("#000000")
            },
            ["Gray"] = new ThemeResources
            {
                Background = Color.Parse("#2E2E2E"),
                Foreground = Color.Parse("#D0D0D0"),
                Border = Color.Parse("#444444"),
                TextBoxBackground = Color.Parse("#333333"),
                TextBoxForeground = Color.Parse("#D0D0D0")
            }
        };

        public string CurrentTheme => _currentTheme;

        public void SetTheme(string themeName)
        {
            if (_themes.ContainsKey(themeName) && _currentTheme != themeName)
            {
                _currentTheme = themeName;
                ThemeChanged?.Invoke();
            }
        }

        public ThemeResources GetResources(string themeName) => 
            _themes.TryGetValue(themeName, out var resources) ? resources : _themes["Dark"];

        public ThemeResources CurrentResources => _themes[_currentTheme];
    }

    public class ThemeResources
    {
        public Color Background { get; set; }
        public Color Foreground { get; set; }
        public Color Border { get; set; }
        public Color TextBoxBackground { get; set; }
        public Color TextBoxForeground { get; set; }
    }
}