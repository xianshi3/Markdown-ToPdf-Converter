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
    /// <summary>
    /// Main ViewModel for the Markdown editor application. Handles file operations, text editing, find/replace, preview, and conversion.
    /// </summary>
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

        /// <summary>
        /// Dynamic window title showing filename and unsaved indicator.
        /// </summary>
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

        /// <summary>Localized tab header for the File section.</summary>
        public string FileTabText => _localization.GetString("file_tab");
        /// <summary>Localized tab header for the Edit section.</summary>
        public string EditTabText => _localization.GetString("edit_tab");
        /// <summary>Localized label for the selected file path display.</summary>
        public string SelectedFileText => _localization.GetString("selected_file");

        /// <summary>Number of lines in the markdown text.</summary>
        public int LineCount
        {
            get => _lineCount;
            set => this.RaiseAndSetIfChanged(ref _lineCount, value);
        }

        /// <summary>Number of words in the markdown text.</summary>
        public int WordCount
        {
            get => _wordCount;
            set => this.RaiseAndSetIfChanged(ref _wordCount, value);
        }

        /// <summary>Total character count of the markdown text.</summary>
        public int CharCount
        {
            get => _charCount;
            set => this.RaiseAndSetIfChanged(ref _charCount, value);
        }

        /// <summary>Current UI theme name (Dark/Light/Gray).</summary>
        public string CurrentTheme
        {
            get => _currentTheme;
            set => this.RaiseAndSetIfChanged(ref _currentTheme, value);
        }

        /// <summary>Indicates whether the document has unsaved changes.</summary>
        public bool HasUnsavedChanges
        {
            get => _hasUnsavedChanges;
            set => this.RaiseAndSetIfChanged(ref _hasUnsavedChanges, value);
        }

        /// <summary>Whether the file sidebar panel is expanded.</summary>
        public bool IsSidebarExpanded
        {
            get => _isSidebarExpanded;
            set
            {
                this.RaiseAndSetIfChanged(ref _isSidebarExpanded, value);
                this.RaisePropertyChanged(nameof(SidebarWidth));
            }
        }

        /// <summary>Computed sidebar width based on expansion state.</summary>
        public double SidebarWidth => IsSidebarExpanded ? 200 : 0;

        /// <summary>Whether the markdown preview panel is visible.</summary>
        public bool IsPreviewVisible
        {
            get => _isPreviewVisible;
            set
            {
                this.RaiseAndSetIfChanged(ref _isPreviewVisible, value);
                UpdatePreview();
            }
        }

        /// <summary>Text for the language toggle button.</summary>
        public string LanguageButtonText => _localization.CurrentLanguage == "zh-CN" ? "English" : "中文";
        /// <summary>Localized status text shown after a file is loaded.</summary>
        public string LoadFileStatusText => _localization.GetString("file_loaded_status");
        /// <summary>Localized text for load failure messages.</summary>
        public string LoadFailedText => _localization.GetString("load_failed");
        /// <summary>Localized hint text shown when no file is loaded.</summary>
        public string DropMdHintText => _localization.GetString("drop_md_hint");

        /// <summary>Font size for the editor text display.</summary>
        public double FontSize
        {
            get => _fontSize;
            set => this.RaiseAndSetIfChanged(ref _fontSize, value);
        }

        /// <summary>Zoom level percentage (50-200). Scales font size proportionally.</summary>
        public double ZoomLevel
        {
            get => _zoomLevel;
            set
            {
                this.RaiseAndSetIfChanged(ref _zoomLevel, value);
                FontSize = Math.Max(8, Math.Min(48, 16 * value / 100));
            }
        }

        /// <summary>Full path of the currently opened markdown file.</summary>
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

        /// <summary>The current markdown text content with undo/redo tracking.</summary>
        public string MarkdownText
        {
            get => _markdownText;
            set
            {
                if (_markdownText != value)
                {
                    // Push current state to undo stack (skip during undo/redo operations)
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
                    // Schedule debounced stats update via timer
                    _statsPending = true;
                    _statsTimer.Stop();
                    _statsTimer.Start();
                    UpdatePreview();
                }
            }
        }

        /// <summary>Status bar message displayed to the user.</summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
        }

        /// <summary>Whether a conversion operation is in progress.</summary>
        public bool IsConverting
        {
            get => _isConverting;
            set => this.RaiseAndSetIfChanged(ref _isConverting, value);
        }

        /// <summary>True when conversion is not busy and markdown text is non-empty.</summary>
        public bool CanConvert => !IsConverting && !string.IsNullOrWhiteSpace(MarkdownText);

        /// <summary>True when the undo stack has entries.</summary>
        public bool CanUndo => _undoStack.Count > 0;
        /// <summary>True when the redo stack has entries.</summary>
        public bool CanRedo => _redoStack.Count > 0;

        /// <summary>Text to search for in the find/replace panel.</summary>
        public string FindText
        {
            get => _findText;
            set
            {
                this.RaiseAndSetIfChanged(ref _findText, value);
                UpdateMatchCount();
            }
        }

        /// <summary>Replacement text for find/replace operations.</summary>
        public string ReplaceText
        {
            get => _replaceText;
            set => this.RaiseAndSetIfChanged(ref _replaceText, value);
        }

        /// <summary>Whether the find/replace panel is visible.</summary>
        public bool IsFindVisible
        {
            get => _isFindVisible;
            set => this.RaiseAndSetIfChanged(ref _isFindVisible, value);
        }

        /// <summary>Whether find/replace should match case sensitivity.</summary>
        public bool MatchCase
        {
            get => _matchCase;
            set
            {
                this.RaiseAndSetIfChanged(ref _matchCase, value);
                UpdateMatchCount();
            }
        }

        /// <summary>Total number of matches found in the current text.</summary>
        public int MatchCount
        {
            get => _matchCount;
            set => this.RaiseAndSetIfChanged(ref _matchCount, value);
        }

        /// <summary>Index (0-based) of the currently highlighted match.</summary>
        public int CurrentMatchIndex
        {
            get => _currentMatchIndex;
            set => this.RaiseAndSetIfChanged(ref _currentMatchIndex, value);
        }

        /// <summary>Formatted match status text (e.g. "2/5") or localized "no matches" message.</summary>
        public string MatchStatusText => MatchCount > 0
            ? $"{CurrentMatchIndex + 1}/{MatchCount}"
            : _localization.GetString("find_no_matches");

        /// <summary>Selected page size for PDF export (e.g. "A4", "Letter").</summary>
        public string PageSize
        {
            get => _pageSize;
            set => this.RaiseAndSetIfChanged(ref _pageSize, value);
        }

        /// <summary>Page margin in mm for PDF export.</summary>
        public double PageMargin
        {
            get => _pageMargin;
            set => this.RaiseAndSetIfChanged(ref _pageMargin, value);
        }

        /// <summary>Font name used in the exported PDF.</summary>
        public string ExportFont
        {
            get => _exportFont;
            set => this.RaiseAndSetIfChanged(ref _exportFont, value);
        }

        /// <summary>Available page size options for PDF export.</summary>
        public ObservableCollection<string> PageSizeOptions { get; } = new() { "A4", "Letter", "Legal", "A3", "A5" };
        /// <summary>Available font options for PDF export.</summary>
        public ObservableCollection<string> FontOptions { get; } = new() { "Inter", "Segoe UI", "Noto Sans", "DejaVu Sans", "Liberation Serif", "Consolas", "SimSun" };

        /// <summary>Parsed preview blocks for the markdown preview panel.</summary>
        public ObservableCollection<PreviewBlock> PreviewBlocks
        {
            get => _previewBlocks;
            set => this.RaiseAndSetIfChanged(ref _previewBlocks, value);
        }

        /// <summary>Recently opened files list (persisted to disk).</summary>
        public ObservableCollection<string> RecentFiles
        {
            get => _recentFiles;
            set => this.RaiseAndSetIfChanged(ref _recentFiles, value);
        }

        /// <summary>Creates a new empty markdown document.</summary>
        public ReactiveCommand<Unit, Unit> NewFileCommand { get; }
        /// <summary>Opens a markdown file via system file picker.</summary>
        public ReactiveCommand<Unit, Unit> OpenFileCommand { get; }
        /// <summary>Saves the current document to its file path.</summary>
        public ReactiveCommand<Unit, Unit> SaveFileCommand { get; }
        /// <summary>Saves the current document with a new file name/path.</summary>
        public ReactiveCommand<Unit, Unit> SaveAsCommand { get; }
        /// <summary>Exports the markdown content to a PDF file.</summary>
        public ReactiveCommand<Unit, Unit> ConvertToPdfCommand { get; }
        /// <summary>Exports the markdown content to an HTML file.</summary>
        public ReactiveCommand<Unit, Unit> ConvertToHtmlCommand { get; }
        /// <summary>Switches between Chinese and English UI localization.</summary>
        public ReactiveCommand<Unit, Unit> SwitchLanguageCommand { get; }
        /// <summary>Cycles through available UI themes (Dark/Light/Gray).</summary>
        public ReactiveCommand<Unit, Unit> SwitchThemeCommand { get; }
        /// <summary>Undoes the last text edit.</summary>
        public ReactiveCommand<Unit, Unit> UndoCommand { get; }
        /// <summary>Redoes the last undone text edit.</summary>
        public ReactiveCommand<Unit, Unit> RedoCommand { get; }
        /// <summary>Toggles the find/replace panel visibility.</summary>
        public ReactiveCommand<Unit, Unit> FindCommand { get; }
        /// <summary>Replaces all occurrences of find text with replace text.</summary>
        public ReactiveCommand<Unit, Unit> ReplaceCommand { get; }
        /// <summary>Jumps to the next find match.</summary>
        public ReactiveCommand<Unit, Unit> FindNextCommand { get; }
        /// <summary>Jumps to the previous find match.</summary>
        public ReactiveCommand<Unit, Unit> FindPrevCommand { get; }
        /// <summary>Hides the find/replace panel.</summary>
        public ReactiveCommand<Unit, Unit> CloseFindCommand { get; }
        /// <summary>Toggles the markdown preview panel.</summary>
        public ReactiveCommand<Unit, Unit> TogglePreviewCommand { get; }
        /// <summary>Toggles the file sidebar panel.</summary>
        public ReactiveCommand<Unit, Unit> ToggleSidebarCommand { get; }
        /// <summary>Shows the About dialog.</summary>
        public ReactiveCommand<Unit, Unit> ShowAboutCommand { get; }
        /// <summary>Inserts bold markdown formatting (**text**).</summary>
        public ReactiveCommand<Unit, Unit> InsertBoldCommand { get; }
        /// <summary>Inserts italic markdown formatting (*text*).</summary>
        public ReactiveCommand<Unit, Unit> InsertItalicCommand { get; }
        /// <summary>Inserts a heading marker (# ) at the start of the document.</summary>
        public ReactiveCommand<Unit, Unit> InsertHeadingCommand { get; }
        /// <summary>Inserts a markdown link [text](url).</summary>
        public ReactiveCommand<Unit, Unit> InsertLinkCommand { get; }
        /// <summary>Inserts a markdown image ![alt](url).</summary>
        public ReactiveCommand<Unit, Unit> InsertImageCommand { get; }
        /// <summary>Wraps selection in inline code backticks.</summary>
        public ReactiveCommand<Unit, Unit> InsertCodeCommand { get; }
        /// <summary>Inserts an unordered list item marker (- ).</summary>
        public ReactiveCommand<Unit, Unit> InsertListCommand { get; }
        /// <summary>Inserts a blockquote marker (> ).</summary>
        public ReactiveCommand<Unit, Unit> InsertQuoteCommand { get; }
        /// <summary>Increases the zoom level by 10%.</summary>
        public ReactiveCommand<Unit, Unit> ZoomInCommand { get; }
        /// <summary>Decreases the zoom level by 10%.</summary>
        public ReactiveCommand<Unit, Unit> ZoomOutCommand { get; }
        /// <summary>Resets zoom level to 100%.</summary>
        public ReactiveCommand<Unit, Unit> ZoomResetCommand { get; }

        /// <summary>
        /// Initializes the MainViewModel with services, timers, commands, and recent files.
        /// </summary>
        public MainViewModel()
        {
            _localization = LocalizationService.Instance;
            _themeService = ThemeService.Instance;
            _converterService = new MarkdownToPdfService();
            _previewService = new MarkdownPreviewService();

            // Debounced timer for updating line/word/char statistics
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

            // Auto-save every 60 seconds if there are unsaved changes
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

            // Convert-to-PDF is allowed when not already converting and there's content to convert
            ConvertToPdfCommand = ReactiveCommand.CreateFromTask(ConvertToPdfAsync,
                this.WhenAnyValue(x => x.IsConverting, x => x.SelectedFilePath, x => x.MarkdownText,
                    (converting, filePath, markdown) =>
                        !converting && (!string.IsNullOrEmpty(filePath) || !string.IsNullOrWhiteSpace(markdown))));

            // Convert-to-HTML requires content and no active conversion
            ConvertToHtmlCommand = ReactiveCommand.CreateFromTask(ConvertToHtmlAsync,
                this.WhenAnyValue(x => x.IsConverting, x => x.MarkdownText,
                    (converting, markdown) => !converting && !string.IsNullOrWhiteSpace(markdown)));

            SwitchLanguageCommand = ReactiveCommand.Create(() =>
            {
                var newLang = _localization.CurrentLanguage == "zh-CN" ? "en-US" : "zh-CN";
                _localization.SetLanguage(newLang);
            });

            // Cycle theme: Dark -> Light -> Gray -> Dark
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
                MarkdownText = (string.IsNullOrEmpty(text) || text.StartsWith('\n') ? "" : "\n\n") + "# ";
            });
            InsertLinkCommand = ReactiveCommand.Create(() => InsertAroundSelection("[", "](url)"));
            InsertImageCommand = ReactiveCommand.Create(() => InsertAroundSelection("![", "](url)"));
            InsertCodeCommand = ReactiveCommand.Create(() => InsertAroundSelection("`", "`"));
            InsertListCommand = ReactiveCommand.Create(() =>
            {
                var text = MarkdownText;
                var hasNewline = text.EndsWith("\n") || text.EndsWith("\r\n") || text.Length == 0;
                MarkdownText = text + (hasNewline ? "" : "\n") + "- ";
            });
            InsertQuoteCommand = ReactiveCommand.Create(() =>
            {
                var text = MarkdownText;
                var hasNewline = text.EndsWith("\n") || text.EndsWith("\r\n") || text.Length == 0;
                MarkdownText = text + (hasNewline ? "" : "\n") + "> ";
            });

            ZoomInCommand = ReactiveCommand.Create(() => { ZoomLevel = Math.Min(200, ZoomLevel + 10); });
            ZoomOutCommand = ReactiveCommand.Create(() => { ZoomLevel = Math.Max(50, ZoomLevel - 10); });
            ZoomResetCommand = ReactiveCommand.Create(() => { ZoomLevel = 100; });

            LoadRecentFiles();
        }

        /// <summary>Wraps the current text with given before/after strings (e.g. **text**).</summary>
        private void InsertAroundSelection(string before, string after)
        {
            var text = MarkdownText;
            if (string.IsNullOrEmpty(text))
            {
                MarkdownText = before + after;
                return;
            }
            // Preserve trailing whitespace when appending formatting
            var trimmed = text.TrimEnd();
            var ws = text.Length - trimmed.Length;
            MarkdownText = trimmed + (trimmed.Length > 0 ? "" : "") + before + after + new string(' ', ws);
        }

        /// <summary>Clears the current document after confirming unsaved changes.</summary>
        private async void NewFile()
        {
            if (HasUnsavedChanges && !string.IsNullOrWhiteSpace(MarkdownText))
            {
                var result = await ShowConfirmDialogAsync(_localization.GetString("confirm_new"));
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

        /// <summary>Shows a confirmation dialog with the given message and returns the result.</summary>
        private static async Task<bool> ShowConfirmDialogAsync(string message)
        {
            var dialog = new Views.ConfirmDialog { Message = message };
            if (WindowService.MainWindow != null)
                return await dialog.ShowDialog<bool>(WindowService.MainWindow);
            return true;
        }

        /// <summary>Opens a system file picker for markdown files and loads the selected file.</summary>
        private async Task OpenFileAsync()
        {
            if (HasUnsavedChanges && !string.IsNullOrWhiteSpace(MarkdownText))
            {
                var proceed = await ShowConfirmDialogAsync(_localization.GetString("confirm_new"));
                if (!proceed) return;
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

        /// <summary>Reads markdown text from the given file path and updates the editor state.</summary>
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

        /// <summary>Saves to the current file path, or prompts for a new path if none is set.</summary>
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

        /// <summary>Opens a save file picker to save the document to a new location.</summary>
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

        /// <summary>Automatically saves unsaved changes every 60 seconds if a file path is known.</summary>
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

        /// <summary>Restores the previous text state from the undo stack.</summary>
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

        /// <summary>Restores the last undone text state from the redo stack.</summary>
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

        /// <summary>Replaces all occurrences of FindText with ReplaceText in the markdown content.</summary>
        private void Replace()
        {
            if (string.IsNullOrEmpty(FindText)) return;
            var comparison = MatchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            MarkdownText = MarkdownText.Replace(FindText, ReplaceText, comparison);
            UpdateMatchCount();
        }

        /// <summary>Advances to the next match, wrapping around to the first match.</summary>
        private void FindNext()
        {
            if (string.IsNullOrEmpty(FindText) || string.IsNullOrWhiteSpace(MarkdownText)) return;
            var comparison = MatchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            // Walk through each match sequentially until we reach the current index, then advance
            int start = 0;
            for (int i = 0; i <= CurrentMatchIndex; i++)
            {
                int idx = MarkdownText.IndexOf(FindText, start, comparison);
                if (idx < 0) { CurrentMatchIndex = 0; return; }
                if (i == CurrentMatchIndex)
                {
                    CurrentMatchIndex = (CurrentMatchIndex + 1) % Math.Max(1, MatchCount);
                    return;
                }
                start = idx + FindText.Length;
            }
            CurrentMatchIndex = 0;
        }

        /// <summary>Moves to the previous match, wrapping around to the last match.</summary>
        private void FindPrev()
        {
            if (string.IsNullOrEmpty(FindText) || string.IsNullOrWhiteSpace(MarkdownText)) return;
            if (MatchCount == 0) return;
            // Decrement with wrap-around using modular arithmetic
            CurrentMatchIndex = (CurrentMatchIndex - 1 + MatchCount) % MatchCount;
        }

        /// <summary>Recalculates the total number of matches and resets the current match index.</summary>
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
            // Count all non-overlapping occurrences of the search text
            while ((index = MarkdownText.IndexOf(FindText, index, comparison)) >= 0)
            {
                count++;
                index += FindText.Length;
            }

            MatchCount = count;
            CurrentMatchIndex = count > 0 ? 0 : 0;
            this.RaisePropertyChanged(nameof(MatchStatusText));
        }

        /// <summary>Recalculates line count, word count, and character count from the markdown text.</summary>
        private void UpdateStatistics()
        {
            LineCount = string.IsNullOrEmpty(MarkdownText) ? 1 : MarkdownText.Replace("\r\n", "\n").Split('\n').Length;
            WordCount = string.IsNullOrWhiteSpace(MarkdownText) ? 0 :
                MarkdownText.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
            CharCount = MarkdownText.Length;
        }

        /// <summary>Reparses the markdown text into preview blocks for the preview panel.</summary>
        private void UpdatePreview()
        {
            var blocks = _previewService.Parse(MarkdownText);
            PreviewBlocks = new ObservableCollection<PreviewBlock>(blocks);
        }

        /// <summary>Refreshes all localized UI strings when the display language changes.</summary>
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
                this.RaisePropertyChanged(nameof(MatchStatusText));
                StatusMessage = _localization.GetString("ready");
            });
        }

        /// <summary>Updates the CurrentTheme property when the theme service fires a change.</summary>
        private void OnThemeChanged()
        {
            Dispatcher.UIThread.Post(() =>
            {
                CurrentTheme = _themeService.CurrentTheme;
            });
        }

        /// <summary>Prompts for a save location and converts markdown to PDF using the selected service.</summary>
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

                // Write PDF directly to the selected file stream
                await using var stream = await file.OpenWriteAsync();
                _converterService.ConvertMarkdownToPdf(MarkdownText, stream, PageSize, PageMargin, ExportFont);

                StatusMessage = $"{_localization.GetString("conversion_success")} {file.Path.LocalPath}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"{_localization.GetString("conversion_failed")} {ex.Message}";
                await ShowErrorDialogAsync(ex);
            }
            finally
            {
                IsConverting = false;
            }
        }

        /// <summary>Displays an error dialog with exception details.</summary>
        private async Task ShowErrorDialogAsync(Exception ex)
        {
            var dialog = new Views.ErrorDialog();
            dialog.ErrorMessage = $"Exception: {ex.GetType().FullName}\nMessage: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}";
            if (WindowService.MainWindow != null)
                await dialog.ShowDialog(WindowService.MainWindow);
            else
                dialog.Show();
        }

        /// <summary>Converts markdown to HTML with an embedded stylesheet and saves to a file.</summary>
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

                // Convert markdown to HTML body using Markdig, then wrap with a styled HTML document
                var html = Markdig.Markdown.ToHtml(MarkdownText);
                var fullHtml = $@"<!DOCTYPE html>
<html lang=""en"">
<head><meta charset=""utf-8""><title>Markdown Export</title>
<style>
body {{ font-family: 'Segoe UI', system-ui, -apple-system, sans-serif; max-width: 900px; margin: 0 auto; padding: 20px; line-height: 1.6; }}
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
                await ShowErrorDialogAsync(ex);
            }
            finally
            {
                IsConverting = false;
            }
        }

        /// <summary>Opens the About dialog showing application information.</summary>
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

        /// <summary>Adds a file path to the top of the recent files list, limited to 10 entries.</summary>
        private void AddRecentFile(string path)
        {
            // Remove duplicate if exists, then insert at top
            var existing = _recentFiles.FirstOrDefault(f => string.Equals(f, path, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
                _recentFiles.Remove(existing);
            _recentFiles.Insert(0, path);
            if (_recentFiles.Count > 10)
                _recentFiles.RemoveAt(_recentFiles.Count - 1);
            SaveRecentFiles();
        }

        /// <summary>Loads the recent files list from a local app data text file.</summary>
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
                    // Only add files that still exist on disk
                    foreach (var f in files)
                    {
                        if (File.Exists(f))
                            _recentFiles.Add(f);
                    }
                }
            }
            catch { }
        }

        /// <summary>Persists the current recent files list to a local app data text file.</summary>
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
