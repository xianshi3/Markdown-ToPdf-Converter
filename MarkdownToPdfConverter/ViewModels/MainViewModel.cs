using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using MarkdownToPdfConverter.Models;
using MarkdownToPdfConverter.Services;

namespace MarkdownToPdfConverter.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly ILocalizationService _localization;
        private readonly IThemeService _themeService;
        private readonly MarkdownToPdfService _converterService;
        private readonly MarkdownPreviewService _previewService;

        private double _fontSize = 16;
        private double _zoomLevel = 100;
        private string _selectedFilePath = string.Empty;
        private string _markdownText = string.Empty;
        private string _statusMessage = string.Empty;
        private bool _isConverting;
        private int _lineCount = 1;
        private int _wordCount = 0;
        private int _charCount = 0;
        private string _currentTheme = "Dark";
        private bool _hasUnsavedChanges = false;
        private bool _isSidebarExpanded = true;

        private string _findText = string.Empty;
        private string _replaceText = string.Empty;
        private bool _isFindVisible = false;
        private bool _matchCase;
        private int _matchCount;
        private int _currentMatchIndex;

        private string _pageSize = "A4";
        private double _pageMargin = 20;
        private string _exportFont = "Segoe UI";

        private readonly Stack<string> _undoStack = new();
        private readonly Stack<string> _redoStack = new();
        private string _lastSavedText = string.Empty;
        private bool _isUndoingRedoing;
        private readonly DispatcherTimer _statsTimer;
        private bool _statsPending;
        private readonly DispatcherTimer _autoSaveTimer;

        private ObservableCollection<string> _recentFiles = new();
        private ObservableCollection<PreviewBlock> _previewBlocks = new();

        private bool _isPreviewVisible;

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
        public string PreviewTabText => _localization.GetString("preview_tab");
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

        public int CharCount
        {
            get => _charCount;
            set => this.RaiseAndSetIfChanged(ref _charCount, value);
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

        public bool IsPreviewVisible
        {
            get => _isPreviewVisible;
            set
            {
                this.RaiseAndSetIfChanged(ref _isPreviewVisible, value);
                UpdatePreview();
            }
        }

        public string LanguageButtonText => _localization.CurrentLanguage == "zh-CN" ? "English" : "中文";
        public string LoadFileStatusText => _localization.GetString("file_loaded_status");
        public string LoadFailedText => _localization.GetString("load_failed");
        public string DropMdHintText => _localization.GetString("drop_md_hint");

        public double FontSize
        {
            get => _fontSize;
            set => this.RaiseAndSetIfChanged(ref _fontSize, value);
        }

        public double ZoomLevel
        {
            get => _zoomLevel;
            set
            {
                this.RaiseAndSetIfChanged(ref _zoomLevel, value);
                FontSize = Math.Max(8, Math.Min(48, 16 * value / 100));
            }
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
                    if (IsPreviewVisible)
                    {
                        UpdatePreview();
                    }
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
            set
            {
                this.RaiseAndSetIfChanged(ref _findText, value);
                UpdateMatchCount();
            }
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

        public bool MatchCase
        {
            get => _matchCase;
            set
            {
                this.RaiseAndSetIfChanged(ref _matchCase, value);
                UpdateMatchCount();
            }
        }

        public int MatchCount
        {
            get => _matchCount;
            set => this.RaiseAndSetIfChanged(ref _matchCount, value);
        }

        public int CurrentMatchIndex
        {
            get => _currentMatchIndex;
            set => this.RaiseAndSetIfChanged(ref _currentMatchIndex, value);
        }

        public string MatchStatusText => MatchCount > 0
            ? $"{CurrentMatchIndex + 1}/{MatchCount}"
            : _localization.GetString("find_no_matches");

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

        public ObservableCollection<PreviewBlock> PreviewBlocks
        {
            get => _previewBlocks;
            set => this.RaiseAndSetIfChanged(ref _previewBlocks, value);
        }

        public ObservableCollection<string> RecentFiles
        {
            get => _recentFiles;
            set => this.RaiseAndSetIfChanged(ref _recentFiles, value);
        }

        public ReactiveCommand<Unit, Unit> NewFileCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenFileCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveFileCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveAsCommand { get; }
        public ReactiveCommand<Unit, Unit> ConvertToPdfCommand { get; }
        public ReactiveCommand<Unit, Unit> ConvertToHtmlCommand { get; }
        public ReactiveCommand<Unit, Unit> SwitchLanguageCommand { get; }
        public ReactiveCommand<Unit, Unit> SwitchThemeCommand { get; }
        public ReactiveCommand<Unit, Unit> UndoCommand { get; }
        public ReactiveCommand<Unit, Unit> RedoCommand { get; }
        public ReactiveCommand<Unit, Unit> FindCommand { get; }
        public ReactiveCommand<Unit, Unit> ReplaceCommand { get; }
        public ReactiveCommand<Unit, Unit> FindNextCommand { get; }
        public ReactiveCommand<Unit, Unit> FindPrevCommand { get; }
        public ReactiveCommand<Unit, Unit> CloseFindCommand { get; }
        public ReactiveCommand<Unit, Unit> TogglePreviewCommand { get; }
        public ReactiveCommand<Unit, Unit> ToggleSidebarCommand { get; }
        public ReactiveCommand<Unit, Unit> ShowAboutCommand { get; }
        public ReactiveCommand<Unit, Unit> InsertBoldCommand { get; }
        public ReactiveCommand<Unit, Unit> InsertItalicCommand { get; }
        public ReactiveCommand<Unit, Unit> InsertHeadingCommand { get; }
        public ReactiveCommand<Unit, Unit> InsertLinkCommand { get; }
        public ReactiveCommand<Unit, Unit> InsertImageCommand { get; }
        public ReactiveCommand<Unit, Unit> InsertCodeCommand { get; }
        public ReactiveCommand<Unit, Unit> InsertListCommand { get; }
        public ReactiveCommand<Unit, Unit> InsertQuoteCommand { get; }
        public ReactiveCommand<Unit, Unit> ZoomInCommand { get; }
        public ReactiveCommand<Unit, Unit> ZoomOutCommand { get; }
        public ReactiveCommand<Unit, Unit> ZoomResetCommand { get; }

        public MainViewModel()
        {
            _localization = LocalizationService.Instance;
            _themeService = ThemeService.Instance;
            _converterService = new MarkdownToPdfService();
            _previewService = new MarkdownPreviewService();

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

            _autoSaveTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(60) };
            _autoSaveTimer.Tick += async (_, _) => await AutoSaveAsync();
            _autoSaveTimer.Start();

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

            ConvertToHtmlCommand = ReactiveCommand.CreateFromTask(ConvertToHtmlAsync,
                this.WhenAnyValue(x => x.IsConverting, x => x.MarkdownText,
                    (converting, markdown) => !converting && !string.IsNullOrWhiteSpace(markdown)));

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
            FindNextCommand = ReactiveCommand.Create(FindNext);
            FindPrevCommand = ReactiveCommand.Create(FindPrev);
            CloseFindCommand = ReactiveCommand.Create(() => { IsFindVisible = false; });

            TogglePreviewCommand = ReactiveCommand.Create(() => { IsPreviewVisible = !IsPreviewVisible; });
            ToggleSidebarCommand = ReactiveCommand.Create(() => { IsSidebarExpanded = !IsSidebarExpanded; });
            ShowAboutCommand = ReactiveCommand.CreateFromTask(ShowAboutAsync);

            InsertBoldCommand = ReactiveCommand.Create(() => InsertAroundSelection("**", "**"));
            InsertItalicCommand = ReactiveCommand.Create(() => InsertAroundSelection("*", "*"));
            InsertHeadingCommand = ReactiveCommand.Create(() =>
            {
                var text = MarkdownText;
                MarkdownText = "# " + text.Insert(0, "# ");
            });
            InsertLinkCommand = ReactiveCommand.Create(() => InsertAroundSelection("[", "](url)"));
            InsertImageCommand = ReactiveCommand.Create(() => InsertAroundSelection("![", "](url)"));
            InsertCodeCommand = ReactiveCommand.Create(() => InsertAroundSelection("`", "`"));
            InsertListCommand = ReactiveCommand.Create(() =>
            {
                var text = MarkdownText;
                MarkdownText = text + (text.EndsWith("\n") || text.Length == 0 ? "" : "\n") + "- ";
            });
            InsertQuoteCommand = ReactiveCommand.Create(() =>
            {
                var text = MarkdownText;
                MarkdownText = text + (text.EndsWith("\n") || text.Length == 0 ? "" : "\n") + "> ";
            });

            ZoomInCommand = ReactiveCommand.Create(() => { ZoomLevel = Math.Min(200, ZoomLevel + 10); });
            ZoomOutCommand = ReactiveCommand.Create(() => { ZoomLevel = Math.Max(50, ZoomLevel - 10); });
            ZoomResetCommand = ReactiveCommand.Create(() => { ZoomLevel = 100; });

            LoadRecentFiles();
        }

        private void InsertAroundSelection(string before, string after)
        {
            var text = MarkdownText;
            MarkdownText = text + before + after;
        }

        private void NewFile()
        {
            if (HasUnsavedChanges && !string.IsNullOrWhiteSpace(MarkdownText))
            {
                var result = ShowConfirmDialog(_localization.GetString("confirm_new"));
                if (!result) return;
            }

            MarkdownText = string.Empty;
            SelectedFilePath = string.Empty;
            _lastSavedText = string.Empty;
            HasUnsavedChanges = false;
            StatusMessage = _localization.GetString("ready");
            this.RaisePropertyChanged(nameof(WindowTitle));
            UpdatePreview();
        }

        private static bool ShowConfirmDialog(string message)
        {
            return true;
        }

        private async Task OpenFileAsync()
        {
            if (HasUnsavedChanges && !string.IsNullOrWhiteSpace(MarkdownText))
            {
                // Prompt would go here; for now just proceed
            }

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
                await LoadFileAsync(file.Path.LocalPath);
            }
        }

        private async Task LoadFileAsync(string path)
        {
            try
            {
                SelectedFilePath = path;
                using var reader = new StreamReader(path);
                MarkdownText = await reader.ReadToEndAsync();
                _lastSavedText = MarkdownText;
                HasUnsavedChanges = false;
                StatusMessage = $"{_localization.GetString("file_loaded")} {Path.GetFileName(path)}";
                AddRecentFile(path);
            }
            catch (Exception ex)
            {
                StatusMessage = $"{_localization.GetString("load_failed")}: {ex.Message}";
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
                    AddRecentFile(file.Path.LocalPath);
                }
                catch (Exception ex)
                {
                    StatusMessage = $"{_localization.GetString("save_failed")}: {ex.Message}";
                }
            }
        }

        private async Task AutoSaveAsync()
        {
            if (!HasUnsavedChanges || string.IsNullOrWhiteSpace(MarkdownText) || string.IsNullOrEmpty(SelectedFilePath))
                return;

            try
            {
                await File.WriteAllTextAsync(SelectedFilePath, MarkdownText);
                _lastSavedText = MarkdownText;
                HasUnsavedChanges = false;
            }
            catch
            {
                // Silently handle auto-save failures
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
            var comparison = MatchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            MarkdownText = MarkdownText.Replace(FindText, ReplaceText, comparison);
            UpdateMatchCount();
        }

        private void FindNext()
        {
            if (string.IsNullOrEmpty(FindText) || string.IsNullOrWhiteSpace(MarkdownText)) return;
            var comparison = MatchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            var index = MarkdownText.IndexOf(FindText, comparison);
            if (index >= 0)
            {
                CurrentMatchIndex = 0;
            }
        }

        private void FindPrev()
        {
            FindNext();
        }

        private void UpdateMatchCount()
        {
            if (string.IsNullOrEmpty(FindText) || string.IsNullOrWhiteSpace(MarkdownText))
            {
                MatchCount = 0;
                CurrentMatchIndex = 0;
                this.RaisePropertyChanged(nameof(MatchStatusText));
                return;
            }

            var comparison = MatchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            int count = 0;
            int index = 0;
            while ((index = MarkdownText.IndexOf(FindText, index, comparison)) >= 0)
            {
                count++;
                index += FindText.Length;
            }

            MatchCount = count;
            CurrentMatchIndex = count > 0 ? 0 : 0;
            this.RaisePropertyChanged(nameof(MatchStatusText));
        }

        private void UpdateStatistics()
        {
            LineCount = string.IsNullOrEmpty(MarkdownText) ? 1 : MarkdownText.Split('\n').Length;
            WordCount = string.IsNullOrWhiteSpace(MarkdownText) ? 0 :
                MarkdownText.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
            CharCount = MarkdownText.Length;
        }

        private void UpdatePreview()
        {
            if (!IsPreviewVisible) return;
            var blocks = _previewService.Parse(MarkdownText);
            PreviewBlocks = new ObservableCollection<PreviewBlock>(blocks);
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
                this.RaisePropertyChanged(nameof(PreviewTabText));
                this.RaisePropertyChanged(nameof(SelectedFileText));
                this.RaisePropertyChanged(nameof(DropMdHintText));
                this.RaisePropertyChanged(nameof(MatchStatusText));
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

        private async Task ConvertToHtmlAsync()
        {
            if (string.IsNullOrWhiteSpace(MarkdownText))
            {
                StatusMessage = _localization.GetString("no_markdown");
                return;
            }

            var file = await WindowService.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = _localization.GetString("save_html"),
                FileTypeChoices = new[] { new FilePickerFileType("HTML Files") { Patterns = new[] { "*.html" } } },
                DefaultExtension = "html"
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

                var html = Markdig.Markdown.ToHtml(MarkdownText);
                var fullHtml = $@"<!DOCTYPE html>
<html lang=""en"">
<head><meta charset=""utf-8""><title>Markdown Export</title>
<style>
body {{ font-family: 'Segoe UI', sans-serif; max-width: 900px; margin: 0 auto; padding: 20px; line-height: 1.6; }}
pre {{ background: #f4f4f4; padding: 12px; border-radius: 6px; overflow-x: auto; }}
code {{ background: #f4f4f4; padding: 2px 6px; border-radius: 3px; }}
table {{ border-collapse: collapse; width: 100%; }}
th, td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
th {{ background: #f4f4f4; }}
blockquote {{ border-left: 4px solid #ddd; margin: 0; padding: 0 16px; color: #666; }}
</style></head>
<body>{html}</body></html>";

                await using var stream = await file.OpenWriteAsync();
                using var writer = new StreamWriter(stream, Encoding.UTF8);
                await writer.WriteAsync(fullHtml);

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

        private async Task ShowAboutAsync()
        {
            var dialog = new Views.AboutWindow();
            if (WindowService.MainWindow != null)
            {
                await dialog.ShowDialog(WindowService.MainWindow);
            }
            else
            {
                dialog.Show();
            }
        }

        private void AddRecentFile(string path)
        {
            if (_recentFiles.Contains(path))
                _recentFiles.Remove(path);
            _recentFiles.Insert(0, path);
            if (_recentFiles.Count > 10)
                _recentFiles.RemoveAt(_recentFiles.Count - 1);
            SaveRecentFiles();
        }

        private void LoadRecentFiles()
        {
            try
            {
                var appData = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "MarkdownToPdfConverter");
                var recentPath = Path.Combine(appData, "recent.txt");
                if (File.Exists(recentPath))
                {
                    var files = File.ReadAllLines(recentPath);
                    foreach (var f in files)
                    {
                        if (File.Exists(f))
                            _recentFiles.Add(f);
                    }
                }
            }
            catch { }
        }

        private void SaveRecentFiles()
        {
            try
            {
                var appData = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "MarkdownToPdfConverter");
                Directory.CreateDirectory(appData);
                var recentPath = Path.Combine(appData, "recent.txt");
                File.WriteAllLines(recentPath, _recentFiles);
            }
            catch { }
        }
    }
}
