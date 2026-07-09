using Avalonia;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace MarkdownToPdfConverter.Services
{
    public interface ILocalizationService
    {
        string CurrentLanguage { get; }
        string GetString(string key);
        void SetLanguage(string languageCode);
        void ApplyToApplication();
        event Action? LanguageChanged;
    }

    public class LocalizationService : ILocalizationService
    {
        public static LocalizationService Instance { get; } = new();

        private Dictionary<string, string> _strings = new();
        private string _currentLanguage = "en-US";

        public string CurrentLanguage => _currentLanguage;
        public event Action? LanguageChanged;

        public LocalizationService()
        {
            var culture = CultureInfo.CurrentUICulture.Name;
            _currentLanguage = culture.StartsWith("zh") ? "zh-CN" : "en-US";
            LoadStrings(_currentLanguage);
        }

        public string GetString(string key)
        {
            return _strings.TryGetValue(key, out var value) ? value : key;
        }

        public void SetLanguage(string languageCode)
        {
            if (_currentLanguage != languageCode)
            {
                _currentLanguage = languageCode;
                LoadStrings(languageCode);
                ApplyToApplication();
                LanguageChanged?.Invoke();
            }
        }

        public void ApplyToApplication()
        {
            var app = Application.Current;
            if (app == null) return;

            foreach (var kvp in _strings)
                app.Resources[$"str_{kvp.Key}"] = kvp.Value;
        }

        private void LoadStrings(string languageCode)
        {
            _strings.Clear();

            var assembly = typeof(LocalizationService).Assembly;
            var resourceName = $"MarkdownToPdfConverter.Resources.Strings.{languageCode}.json";

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                resourceName = "MarkdownToPdfConverter.Resources.Strings.en-US.json";
                using var fallbackStream = assembly.GetManifestResourceStream(resourceName);
                if (fallbackStream == null) return;
                LoadFromStream(fallbackStream);
                return;
            }

            LoadFromStream(stream);
        }

        private void LoadFromStream(Stream stream)
        {
            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            if (dict != null)
            {
                foreach (var kvp in dict)
                {
                    _strings[kvp.Key] = kvp.Value;
                }
            }
        }
    }
}
