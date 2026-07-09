using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using MarkdownToPdfConverter.Services;

namespace MarkdownToPdfConverter.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly ILocalizationService _localization;
        private readonly IThemeService _themeService;
        private readonly MarkdownToPdfService _converterService;

        private double _fontSize = 16;
        private string _selectedFilePath = string.Empty;
        private string _markdownText = string.Empty;
        private string _statusMessage = string.Empty;
        private bool _isConverting;
        private int _lineCount = 1;
        private int _wordCount = 0;
        private string _currentTheme = "Dark";
        private bool _hasUnsavedChanges = false;
        private bool _isSidebarExpanded = true;

        private string _findText = string.Empty;
        private string _replaceText = string.Empty;
        private bool _isFindVisible = false;

        private string _pageSize = "A4";
        private double _pageMargin = 20;
        private string _exportFont = "Segoe UI";

        private readonly Stack<string> _undoStack = new();
        private readonly Stack<string> _redoStack = new();
        private string _lastSavedText = string.Empty;
        private bool _isUndoingRedoing;
        private readonly DispatcherTimer _statsTimer;
        private bool _statsPending;

        public string WindowTitle
        {
            get
            {
                var title = _localization.GetString("app_title");
                if (!string.IsNullOrEmpty(SelectedFilePath))
                    title = $"{Path.GetFileName(SelectedFilePath)} - {title}";
                if (HasUnsavedChanges)
                    title = $"*{title}";
                return title;
            }
        }

        public string FileTabText => _localization.GetString("file_tab");
        public string EditTabText => _localization.GetString("edit_tab");
        public string SelectedFileText => _localization.GetString("selected_file");

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

        public bool HasUnsavedChanges
        {
            get => _hasUnsavedChanges;
            set => this.RaiseAndSetIfChanged(ref _hasUnsavedChanges, value);
        }

        public bool IsSidebarExpanded
        {
            get => _isSidebarExpanded;
            set
            {
                this.RaiseAndSetIfChanged(ref _isSidebarExpanded, value);
                this.RaisePropertyChanged(nameof(SidebarWidth));
            }
        }

        public double SidebarWidth => IsSidebarExpanded ? 200 : 0;

        public string LanguageButtonText => _localization.CurrentLanguage == "zh-CN" ? "English" : "中文";
        public string LoadFileStatusText => _localization.GetString("file_loaded_status");
        public string LoadFailedText => _localization.GetString("load_failed");
        public string DropMdHintText => _localization.GetString("drop_md_hint");

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
                this.RaisePropertyChanged(nameof(WindowTitle));
            }
        }

        public string MarkdownText
        {
            get => _markdownText;
            set
            {
                if (_markdownText != value)
                {
                    if (!_isUndoingRedoing)
                    {
                        _undoStack.Push(_markdownText);
                        _redoStack.Clear();
                    }
                    this.RaiseAndSetIfChanged(ref _markdownText, value);
                    this.RaisePropertyChanged(nameof(CanConvert));
                    this.RaisePropertyChanged(nameof(WindowTitle));
                    this.RaisePropertyChanged(nameof(CanUndo));
                    this.RaisePropertyChanged(nameof(CanRedo));
                    HasUnsavedChanges = value != _lastSavedText;
                    _statsPending = true;
                    _statsTimer.Stop();
                    _statsTimer.Start();
                }
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

        public bool CanConvert => !IsConverting && !string.IsNullOrWhiteSpace(MarkdownText);

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;

        public string FindText
        {
            get => _findText;
            set => this.RaiseAndSetIfChanged(ref _findText, value);
        }

        public string ReplaceText
        {
            get => _replaceText;
            set => this.RaiseAndSetIfChanged(ref _replaceText, value);
        }

        public bool IsFindVisible
        {
            get => _isFindVisible;
            set => this.RaiseAndSetIfChanged(ref _isFindVisible, value);
        }

        public string PageSize
        {
            get => _pageSize;
            set => this.RaiseAndSetIfChanged(ref _pageSize, value);
        }

        public double PageMargin
        {
            get => _pageMargin;
            set => this.RaiseAndSetIfChanged(ref _pageMargin, value);
        }

        public string ExportFont
        {
            get => _exportFont;
            set => this.RaiseAndSetIfChanged(ref _exportFont, value);
        }

        public ObservableCollection<string> PageSizeOptions { get; } = new() { "A4", "Letter", "Legal", "A3", "A5" };
        public ObservableCollection<string> FontOptions { get; } = new() { "Segoe UI", "Arial", "Times New Roman", "Consolas", "Microsoft YaHei" };

        public ReactiveCommand<Unit, Unit> NewFileCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenFileCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveFileCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveAsCommand { get; }
        public ReactiveCommand<Unit, Unit> ConvertToPdfCommand { get; }
        public ReactiveCommand<Unit, Unit> SwitchLanguageCommand { get; }
        public ReactiveCommand<Unit, Unit> SwitchThemeCommand { get; }
        public ReactiveCommand<Unit, Unit> UndoCommand { get; }
        public ReactiveCommand<Unit, Unit> RedoCommand { get; }
        public ReactiveCommand<Unit, Unit> FindCommand { get; }
        public ReactiveCommand<Unit, Unit> ReplaceCommand { get; }
        public ReactiveCommand<Unit, Unit> CloseFindCommand { get; }
        public ReactiveCommand<Unit, Unit> ToggleSidebarCommand { get; }

        public MainViewModel()
        {
            _localization = LocalizationService.Instance;
            _themeService = ThemeService.Instance;
            _converterService = new MarkdownToPdfService();

            _statsTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
            _statsTimer.Tick += (_, _) =>
            {
                _statsTimer.Stop();
                if (_statsPending)
                {
                    _statsPending = false;
                    UpdateStatistics();
                }
            };

            CurrentTheme = _themeService.CurrentTheme;
            StatusMessage = _localization.GetString("ready");

            _localization.LanguageChanged += OnLanguageChanged;
            _themeService.ThemeChanged += OnThemeChanged;

            NewFileCommand = ReactiveCommand.Create(NewFile);
            OpenFileCommand = ReactiveCommand.CreateFromTask(OpenFileAsync);
            SaveFileCommand = ReactiveCommand.CreateFromTask(SaveFileAsync);
            SaveAsCommand = ReactiveCommand.CreateFromTask(SaveAsAsync);

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

            UndoCommand = ReactiveCommand.Create(Undo);
            RedoCommand = ReactiveCommand.Create(Redo);
            FindCommand = ReactiveCommand.Create(() => { IsFindVisible = !IsFindVisible; });
            ReplaceCommand = ReactiveCommand.Create(Replace);
            CloseFindCommand = ReactiveCommand.Create(() => { IsFindVisible = false; });
            ToggleSidebarCommand = ReactiveCommand.Create(() => { IsSidebarExpanded = !IsSidebarExpanded; });
        }

        private void NewFile()
        {
            MarkdownText = string.Empty;
            SelectedFilePath = string.Empty;
            _lastSavedText = string.Empty;
            HasUnsavedChanges = false;
            StatusMessage = _localization.GetString("ready");
            this.RaisePropertyChanged(nameof(WindowTitle));
        }

        private async Task OpenFileAsync()
        {
            var files = await WindowService.OpenFilePickerAsync(new FilePickerOpenOptions
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
                _lastSavedText = MarkdownText;
                HasUnsavedChanges = false;
                StatusMessage = _localization.GetString("file_loaded");
            }
        }

        private async Task SaveFileAsync()
        {
            if (string.IsNullOrEmpty(SelectedFilePath))
            {
                await SaveAsAsync();
                return;
            }

            try
            {
                await File.WriteAllTextAsync(SelectedFilePath, MarkdownText);
                _lastSavedText = MarkdownText;
                HasUnsavedChanges = false;
                StatusMessage = $"{_localization.GetString("saved_to")} {SelectedFilePath}";
                this.RaisePropertyChanged(nameof(WindowTitle));
            }
            catch (Exception ex)
            {
                StatusMessage = $"{_localization.GetString("save_failed")}: {ex.Message}";
            }
        }

        private async Task SaveAsAsync()
        {
            var file = await WindowService.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save Markdown File",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("Markdown Files") { Patterns = new[] { "*.md" } },
                    new FilePickerFileType("All Files") { Patterns = new[] { "*" } }
                },
                DefaultExtension = "md",
                SuggestedFileName = string.IsNullOrEmpty(SelectedFilePath) ? "untitled.md" : Path.GetFileName(SelectedFilePath)
            });

            if (file != null)
            {
                SelectedFilePath = file.Path.LocalPath;
                try
                {
                    await using var stream = await file.OpenWriteAsync();
                    using var writer = new StreamWriter(stream);
                    await writer.WriteAsync(MarkdownText);
                    _lastSavedText = MarkdownText;
                    HasUnsavedChanges = false;
                    StatusMessage = $"{_localization.GetString("saved_to")} {file.Path.LocalPath}";
                    this.RaisePropertyChanged(nameof(WindowTitle));
                }
                catch (Exception ex)
                {
                    StatusMessage = $"{_localization.GetString("save_failed")}: {ex.Message}";
                }
            }
        }

        private void Undo()
        {
            if (_undoStack.Count > 0)
            {
                _isUndoingRedoing = true;
                _redoStack.Push(MarkdownText);
                MarkdownText = _undoStack.Pop();
                _isUndoingRedoing = false;
            }
        }

        private void Redo()
        {
            if (_redoStack.Count > 0)
            {
                _isUndoingRedoing = true;
                _undoStack.Push(MarkdownText);
                MarkdownText = _redoStack.Pop();
                _isUndoingRedoing = false;
            }
        }

        private void Replace()
        {
            if (string.IsNullOrEmpty(FindText)) return;
            MarkdownText = MarkdownText.Replace(FindText, ReplaceText);
        }

        private void UpdateStatistics()
        {
            LineCount = string.IsNullOrEmpty(MarkdownText) ? 1 : MarkdownText.Split('\n').Length;
            WordCount = string.IsNullOrWhiteSpace(MarkdownText) ? 0 :
                MarkdownText.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        private void OnLanguageChanged()
        {
            Dispatcher.UIThread.Post(() =>
            {
                this.RaisePropertyChanged(nameof(WindowTitle));
                this.RaisePropertyChanged(nameof(LanguageButtonText));
                this.RaisePropertyChanged(nameof(LoadFileStatusText));
                this.RaisePropertyChanged(nameof(LoadFailedText));
                this.RaisePropertyChanged(nameof(FileTabText));
                this.RaisePropertyChanged(nameof(EditTabText));
                this.RaisePropertyChanged(nameof(SelectedFileText));
                this.RaisePropertyChanged(nameof(DropMdHintText));
                StatusMessage = _localization.GetString("ready");
            });
        }

        private void OnThemeChanged()
        {
            Dispatcher.UIThread.Post(() =>
            {
                CurrentTheme = _themeService.CurrentTheme;
            });
        }

        private async Task ConvertToPdfAsync()
        {
            if (string.IsNullOrWhiteSpace(MarkdownText))
            {
                StatusMessage = _localization.GetString("no_markdown");
                return;
            }

            var file = await WindowService.SaveFilePickerAsync(new FilePickerSaveOptions
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
                StatusMessage = _localization.GetString("converting");

                await using var stream = await file.OpenWriteAsync();
                _converterService.ConvertMarkdownToPdf(MarkdownText, stream, PageSize, PageMargin, ExportFont);

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

    }
}
