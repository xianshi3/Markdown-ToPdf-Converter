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
using System.Collections.ObjectModel;

namespace MarkdownToPdfConverter.ViewModels
{
    public class MainViewModel : ReactiveObject
    {
        private readonly ILocalizationService _localization;
        private readonly IThemeService _themeService;
        private readonly MarkdownToPdfService _converterService;
        private readonly MarkdownPreviewService _previewService;

        private double _fontSize = 16;
        private string _selectedFilePath = string.Empty;
        private string _markdownText = string.Empty;
        private string _statusMessage = string.Empty;
        private bool _isConverting;
        private ObservableCollection<PreviewBlock> _previewBlocks = new();
        private int _lineCount = 1;
        private int _wordCount = 0;
        private string _currentTheme = "Dark";

        // 主题属性 - 使用 setter 让它们可以被 RaiseAndSetIfChanged
        private IBrush _background = Brushes.Transparent;
        private IBrush _foreground = Brushes.White;
        private IBrush _borderBrush = Brushes.Gray;
        private IBrush _textBoxBackground = Brushes.Black;
        private IBrush _textBoxForeground = Brushes.White;

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
            get => _background;
            set => this.RaiseAndSetIfChanged(ref _background, value);
        }

        public IBrush Foreground
        {
            get => _foreground;
            set => this.RaiseAndSetIfChanged(ref _foreground, value);
        }

        public IBrush BorderBrush
        {
            get => _borderBrush;
            set => this.RaiseAndSetIfChanged(ref _borderBrush, value);
        }

        public IBrush TextBoxBackground
        {
            get => _textBoxBackground;
            set => this.RaiseAndSetIfChanged(ref _textBoxBackground, value);
        }

public IBrush TextBoxForeground
        {
            get => _textBoxForeground;
            set => this.RaiseAndSetIfChanged(ref _textBoxForeground, value);
        }

        public ObservableCollection<PreviewBlock> PreviewBlocks
        {
            get => _previewBlocks;
            set => this.RaiseAndSetIfChanged(ref _previewBlocks, value);
        }

        public int LineCount
        {
            get => _lineCount;
            set => this.RaiseAndSetIfChanged(ref _lineCount, value);
        }

        public int WordCount
        {
            get => _wordCount;
            set => this.RaiseAndSetIfChanged(ref _wordCount, value);
        }

        public string CurrentTheme
        {
            get => _currentTheme;
            set => this.RaiseAndSetIfChanged(ref _currentTheme, value);
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
                UpdatePreview();
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

        public bool CanConvert => !IsConverting && 
            (!string.IsNullOrEmpty(SelectedFilePath) || !string.IsNullOrWhiteSpace(MarkdownText));

        public ReactiveCommand<Unit, Unit> UploadFileCommand { get; }
        public ReactiveCommand<Unit, Unit> ConvertToPdfCommand { get; }
        public ReactiveCommand<Unit, Unit> SwitchLanguageCommand { get; }
        public ReactiveCommand<Unit, Unit> SwitchThemeCommand { get; }

public MainViewModel()
        {
            _localization = new LocalizationService();
            _themeService = new ThemeService();
            _converterService = new MarkdownToPdfService();
            _previewService = new MarkdownPreviewService();

            CurrentTheme = _themeService.CurrentTheme;
            ApplyThemeResources();

            StatusMessage = _localization.GetString("ready");

            _localization.LanguageChanged += OnLanguageChanged;
            _themeService.ThemeChanged += OnThemeChanged;

            UploadFileCommand = ReactiveCommand.CreateFromTask(UploadFileAsync);

            ConvertToPdfCommand = ReactiveCommand.CreateFromTask(ConvertToPdfAsync,
                this.WhenAnyValue(x => x.IsConverting, x => x.SelectedFilePath, x => x.MarkdownText,
                    (converting, filePath, markdown) => 
                        !converting && (!string.IsNullOrEmpty(filePath) || !string.IsNullOrWhiteSpace(markdown))));

            SwitchLanguageCommand = ReactiveCommand.Create(() =>
            {
                var newLang = _localization.CurrentLanguage == "zh-CN" ? "en-US" : "zh-CN";
                _localization.SetLanguage(newLang);
            });

            SwitchThemeCommand = ReactiveCommand.Create(() =>
            {
                var newTheme = _themeService.CurrentTheme switch
                {
                    "Dark" => "Light",
                    "Light" => "Gray",
                    "Gray" => "Dark",
                    _ => "Dark"
                };
                _themeService.SetTheme(newTheme);
            });
        }

        private void ApplyThemeResources()
        {
            var resources = _themeService.CurrentResources;
            Background = resources.Background;
            Foreground = resources.Foreground;
            BorderBrush = resources.BorderBrush;
            TextBoxBackground = resources.TextBoxBackground;
            TextBoxForeground = resources.TextBoxForeground;
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
                // StatusMessage might need to be re-translated
                StatusMessage = _localization.GetString("ready");
            });
        }

private void OnThemeChanged()
        {
            Dispatcher.UIThread.Post(() =>
            {
                CurrentTheme = _themeService.CurrentTheme;
                ApplyThemeResources();
            });
        }

        private void UpdatePreview()
        {
            var blocks = _previewService.ParseToBlocks(MarkdownText);
            PreviewBlocks = new ObservableCollection<PreviewBlock>(blocks);
            
            LineCount = string.IsNullOrEmpty(MarkdownText) ? 1 : MarkdownText.Split('\n').Length;
            WordCount = string.IsNullOrWhiteSpace(MarkdownText) ? 0 : 
                MarkdownText.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
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