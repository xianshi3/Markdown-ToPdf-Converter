using Avalonia;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace MarkdownToPdfConverter.Services
{
    /// <summary>
    /// Provides localized string resources with language switching support.
    /// </summary>
    public interface ILocalizationService
    {
        string CurrentLanguage { get; }
        string GetString(string key);
        void SetLanguage(string languageCode);
        void ApplyToApplication();
        event Action? LanguageChanged;
    }

    /// <summary>
    /// Loads JSON-based string resources by language code and applies them to the application.
    /// </summary>
    public class LocalizationService : ILocalizationService
    {
        public static LocalizationService Instance { get; } = new();

        private Dictionary<string, string> _strings = new();
        private string _currentLanguage = "en-US";

        public string CurrentLanguage => _currentLanguage;
        public event Action? LanguageChanged;

        /// <summary>
        /// Auto-detects the system UI culture and loads the matching (or default) language strings.
        /// </summary>
        public LocalizationService()
        {
            var culture = CultureInfo.CurrentUICulture.Name;
            _currentLanguage = culture.StartsWith("zh") ? "zh-CN" : "en-US";
            LoadStrings(_currentLanguage);
        }

        /// <summary>
        /// Returns the localized string for the given key, or the key itself if not found.
        /// </summary>
        public string GetString(string key)
        {
            return _strings.TryGetValue(key, out var value) ? value : key;
        }

        /// <summary>
        /// Changes the current language, reloads strings, and updates the application.
        /// </summary>
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

        /// <summary>
        /// Applies all loaded string resources to the application's resource dictionary with a "str_" prefix.
        /// </summary>
        public void ApplyToApplication()
        {
            var app = Application.Current;
            if (app == null) return;

            foreach (var kvp in _strings)
                app.Resources[$"str_{kvp.Key}"] = kvp.Value;
        }

        /// <summary>
        /// Loads string resources from the embedded JSON file for the given language code.
        /// Falls back to en-US if the requested language file is not found.
        /// </summary>
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

        /// <summary>
        /// Deserializes a JSON stream into the strings dictionary.
        /// </summary>
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
