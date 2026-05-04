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
                Background = new SolidColorBrush(Color.Parse("#1E1E2E")),
                SurfaceBackground = new SolidColorBrush(Color.Parse("#2D2D44")),
                Foreground = new SolidColorBrush(Color.Parse("#E0E0E0")),
                SecondaryForeground = new SolidColorBrush(Color.Parse("#8888AA")),
                BorderBrush = new SolidColorBrush(Color.Parse("#3D3D5C")),
                AccentBrush = new SolidColorBrush(Color.Parse("#6C5CE7")),
                AccentHoverBrush = new SolidColorBrush(Color.Parse("#7C6CF7")),
                TextBoxBackground = new SolidColorBrush(Color.Parse("#1E1E2E")),
                TextBoxForeground = new SolidColorBrush(Color.Parse("#E0E0E0")),
                ButtonBackground = new SolidColorBrush(Color.Parse("#2D2D44")),
                ButtonHoverBackground = new SolidColorBrush(Color.Parse("#3D3D5C")),
                WindowBorderBrush = new SolidColorBrush(Color.Parse("#2D2D44")),
                StatusBarBackground = new SolidColorBrush(Color.Parse("#1E1E2E")),
                TabItemForeground = new SolidColorBrush(Color.Parse("#8888AA")),
                TabItemSelectedForeground = new SolidColorBrush(Color.Parse("#6C5CE7")),
                PrimaryAccent = new SolidColorBrush(Color.Parse("#6C5CE7")),
                PrimaryAccentHover = new SolidColorBrush(Color.Parse("#A855F7"))
            },
            ["Light"] = new ThemeResources
            {
                Background = new SolidColorBrush(Color.Parse("#FFFFFF")),
                SurfaceBackground = new SolidColorBrush(Color.Parse("#F5F5F5")),
                Foreground = new SolidColorBrush(Color.Parse("#303030")),
                SecondaryForeground = new SolidColorBrush(Color.Parse("#888888")),
                BorderBrush = new SolidColorBrush(Color.Parse("#E0E0E0")),
                AccentBrush = new SolidColorBrush(Color.Parse("#6C5CE7")),
                AccentHoverBrush = new SolidColorBrush(Color.Parse("#7C6CF7")),
                TextBoxBackground = new SolidColorBrush(Color.Parse("#FFFFFF")),
                TextBoxForeground = new SolidColorBrush(Color.Parse("#000000")),
                ButtonBackground = new SolidColorBrush(Color.Parse("#F0F0F0")),
                ButtonHoverBackground = new SolidColorBrush(Color.Parse("#E0E0E0")),
                WindowBorderBrush = new SolidColorBrush(Color.Parse("#E0E0E0")),
                StatusBarBackground = new SolidColorBrush(Color.Parse("#F5F5F5")),
                TabItemForeground = new SolidColorBrush(Color.Parse("#888888")),
                TabItemSelectedForeground = new SolidColorBrush(Color.Parse("#6C5CE7")),
                PrimaryAccent = new SolidColorBrush(Color.Parse("#6C5CE7")),
                PrimaryAccentHover = new SolidColorBrush(Color.Parse("#A855F7"))
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
                WindowBorderBrush = new SolidColorBrush(Color.Parse("#444444")),
                StatusBarBackground = new SolidColorBrush(Color.Parse("#363636")),
                TabItemForeground = new SolidColorBrush(Color.Parse("#888888")),
                TabItemSelectedForeground = new SolidColorBrush(Color.Parse("#6C5CE7")),
                PrimaryAccent = new SolidColorBrush(Color.Parse("#6C5CE7")),
                PrimaryAccentHover = new SolidColorBrush(Color.Parse("#A855F7"))
            }
        };

        public string CurrentTheme => _currentTheme;
        public ThemeResources CurrentResources => _themes[_currentTheme];

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
    }

    public class ThemeResources
    {
        // 主要颜色
        public IBrush Background { get; set; } = Brushes.Transparent;
        public IBrush SurfaceBackground { get; set; } = Brushes.Transparent;
        public IBrush Foreground { get; set; } = Brushes.White;
        public IBrush SecondaryForeground { get; set; } = Brushes.Gray;
        public IBrush BorderBrush { get; set; } = Brushes.Gray;
        
        // 强调色
        public IBrush AccentBrush { get; set; } = Brushes.Blue;
        public IBrush AccentHoverBrush { get; set; } = Brushes.LightBlue;
        public IBrush PrimaryAccent { get; set; } = Brushes.Purple;
        public IBrush PrimaryAccentHover { get; set; } = Brushes.MediumPurple;
        
        // 文本框颜色
        public IBrush TextBoxBackground { get; set; } = Brushes.Black;
        public IBrush TextBoxForeground { get; set; } = Brushes.White;
        
        // 按钮颜色
        public IBrush ButtonBackground { get; set; } = Brushes.DarkGray;
        public IBrush ButtonHoverBackground { get; set; } = Brushes.Gray;
        
        // 窗口元素
        public IBrush WindowBorderBrush { get; set; } = Brushes.Gray;
        public IBrush StatusBarBackground { get; set; } = Brushes.DarkGray;
        
        // Tab颜色
        public IBrush TabItemForeground { get; set; } = Brushes.Gray;
        public IBrush TabItemSelectedForeground { get; set; } = Brushes.Purple;
    }
}