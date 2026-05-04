using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using ReactiveUI;
using System;
using System.Reactive;
using System.Threading.Tasks;
using MarkdownToPdfConverter.Services;
using System.IO;
using Avalonia.Platform.Storage;
using Avalonia.Threading;

namespace MarkdownToPdfConverter.ViewModels
{
    public class MainViewModel : ReactiveObject
    {
        private readonly ILocalizationService _localization;
        private readonly IThemeService _themeService;
        private readonly MarkdownToPdfService _converterService;

        private string _theme = "Dark";
        private double _fontSize = 16;
        private string _selectedFilePath = string.Empty;
        private string _markdownText = string.Empty;
        private string _statusMessage = string.Empty;
        private bool _isConverting;

        public string WindowTitle => _localization.GetString("app_title");
        public string UploadButtonText => _localization.GetString("upload");
        public string ConvertButtonText => _localization.GetString("convert");
        public string HelpButtonText => _localization.GetString("help");
        public string FileTabText => _localization.GetString("file_tab");
        public string EditTabText => _localization.GetString("edit_tab");
        public string SelectedFileText => _localization.GetString("selected_file");
        public string EditContentText => _localization.GetString("edit_content");

        public IBrush Background
        {
            get => new SolidColorBrush(_themeService.GetResources(_theme).Background);
        }

        public IBrush Foreground
        {
            get => new SolidColorBrush(_themeService.GetResources(_theme).Foreground);
        }

        public IBrush BorderBrush
        {
            get => new SolidColorBrush(_themeService.GetResources(_theme).Border);
        }

        public IBrush TextBoxBackground
        {
            get => new SolidColorBrush(_themeService.GetResources(_theme).TextBoxBackground);
        }

        public IBrush TextBoxForeground
        {
            get => new SolidColorBrush(_themeService.GetResources(_theme).TextBoxForeground);
        }

        public string LanguageButtonText => _localization.CurrentLanguage == "zh-CN" ? "English" : "中文";

        public double FontSize
        {
            get => _fontSize;
            set => this.RaiseAndSetIfChanged(ref _fontSize, value);
        }

        public string SelectedFilePath
        {
            get => _selectedFilePath;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedFilePath, value);
                this.RaisePropertyChanged(nameof(CanConvert));
            }
        }

        public string MarkdownText
        {
            get => _markdownText;
            set
            {
                this.RaiseAndSetIfChanged(ref _markdownText, value);
                this.RaisePropertyChanged(nameof(CanConvert));
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
        }

        public bool IsConverting
        {
            get => _isConverting;
            set => this.RaiseAndSetIfChanged(ref _isConverting, value);
        }

        public bool CanConvert => !IsConverting && (!string.IsNullOrEmpty(SelectedFilePath) || !string.IsNullOrWhiteSpace(MarkdownText));

        public ReactiveCommand<Unit, Unit> UploadFileCommand { get; }
        public ReactiveCommand<Unit, Unit> ConvertToPdfCommand { get; }
        public ReactiveCommand<Unit, Unit> SwitchLanguageCommand { get; }
        public ReactiveCommand<Unit, Unit> SwitchThemeCommand { get; }

        public MainViewModel()
        {
            _localization = new LocalizationService();
            _themeService = new ThemeService();
            _converterService = new MarkdownToPdfService();

            StatusMessage = _localization.GetString("ready");

            _localization.LanguageChanged += OnLanguageChanged;
            _themeService.ThemeChanged += OnThemeChanged;

            UploadFileCommand = ReactiveCommand.CreateFromTask(UploadFileAsync);

            ConvertToPdfCommand = ReactiveCommand.CreateFromTask(ConvertToPdfAsync,
                this.WhenAnyValue(x => x.IsConverting, x => x.SelectedFilePath, x => x.MarkdownText,
                    (converting, filePath, markdown) => !converting && (!string.IsNullOrEmpty(filePath) || !string.IsNullOrWhiteSpace(markdown))));

            SwitchLanguageCommand = ReactiveCommand.Create(() =>
            {
                var newLang = _localization.CurrentLanguage == "zh-CN" ? "en-US" : "zh-CN";
                _localization.SetLanguage(newLang);
            });

            SwitchThemeCommand = ReactiveCommand.Create(() =>
            {
                _theme = _theme switch
                {
                    "Dark" => "Light",
                    "Light" => "Gray",
                    _ => "Dark"
                };
                _themeService.SetTheme(_theme);
            });
        }

        private void OnLanguageChanged()
        {
            Dispatcher.UIThread.Post(() =>
            {
                this.RaisePropertyChanged(nameof(WindowTitle));
                this.RaisePropertyChanged(nameof(UploadButtonText));
                this.RaisePropertyChanged(nameof(ConvertButtonText));
                this.RaisePropertyChanged(nameof(HelpButtonText));
                this.RaisePropertyChanged(nameof(FileTabText));
                this.RaisePropertyChanged(nameof(EditTabText));
                this.RaisePropertyChanged(nameof(SelectedFileText));
                this.RaisePropertyChanged(nameof(EditContentText));
                this.RaisePropertyChanged(nameof(LanguageButtonText));
                this.RaisePropertyChanged(nameof(StatusMessage));
            });
        }

        private void OnThemeChanged()
        {
            Dispatcher.UIThread.Post(() =>
            {
                this.RaisePropertyChanged(nameof(Background));
                this.RaisePropertyChanged(nameof(Foreground));
                this.RaisePropertyChanged(nameof(BorderBrush));
                this.RaisePropertyChanged(nameof(TextBoxBackground));
                this.RaisePropertyChanged(nameof(TextBoxForeground));
            });
        }

        private async Task UploadFileAsync()
        {
            var window = GetMainWindow();
            if (window == null)
            {
                StatusMessage = _localization.GetString("no_window");
                return;
            }

            var files = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = _localization.GetString("select_markdown"),
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("Markdown Files") { Patterns = new[] { "*.md", "*.markdown" } },
                    new FilePickerFileType("All Files") { Patterns = new[] { "*" } }
                },
                AllowMultiple = false
            });

            if (files.Count > 0)
            {
                var file = files[0];
                SelectedFilePath = file.Path.LocalPath;
                await using var stream = await file.OpenReadAsync();
                using var reader = new StreamReader(stream);
                MarkdownText = await reader.ReadToEndAsync();
                StatusMessage = _localization.GetString("file_loaded");
            }
        }

        private async Task ConvertToPdfAsync()
        {
            if (string.IsNullOrWhiteSpace(MarkdownText))
            {
                StatusMessage = _localization.GetString("no_markdown");
                return;
            }

            var window = GetMainWindow();
            if (window == null)
            {
                StatusMessage = _localization.GetString("no_window");
                return;
            }

            var file = await window.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = _localization.GetString("save_pdf"),
                FileTypeChoices = new[] { new FilePickerFileType("PDF Files") { Patterns = new[] { "*.pdf" } } },
                DefaultExtension = "pdf"
            });

            if (file == null)
            {
                StatusMessage = _localization.GetString("save_cancelled");
                return;
            }

            try
            {
                IsConverting = true;
                StatusMessage = "...";

                await using var stream = await file.OpenWriteAsync();
                _converterService.ConvertMarkdownToPdf(MarkdownText, stream);

                StatusMessage = $"{_localization.GetString("conversion_success")} {file.Path.LocalPath}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"{_localization.GetString("conversion_failed")} {ex.Message}";
            }
            finally
            {
                IsConverting = false;
            }
        }

        private Window? GetMainWindow()
        {
            var topLevel = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            return topLevel?.MainWindow;
        }
    }
}