using Avalonia;
using Avalonia.Controls;
using ReactiveUI;
using System;
using System.Reactive;
using System.Threading.Tasks;
using MarkdownToPdfConverter.Services;
using System.IO;
using Avalonia.Platform.Storage;

namespace MarkdownToPdfConverter.ViewModels
{
    public class MainViewModel : ReactiveObject
    {
        private bool _isChinese = true; // 默认中文
        private string _theme = "Gray"; // 默认灰主题
        private string _background;
        private string _foreground;
        private string _borderBrush;
        private string _textBoxBackground;
        private string _textBoxForeground;
        private double _fontSize = 16; // 默认字体大小

        public ReactiveCommand<Unit, Unit> SwitchLanguageCommand { get; }
        public ReactiveCommand<Unit, Unit> SwitchThemeCommand { get; }

        // 添加语言相关属性
        public string LanguageButtonText => _isChinese ? "English" : "中文";
        public string UploadButtonText => _isChinese ? "上传 Markdown" : "Upload Markdown";
        public string ConvertButtonText => _isChinese ? "转换 PDF" : "Convert PDF";
        public string HelpButtonText => _isChinese ? "帮助" : "Help";
        public string FileTabText => _isChinese ? "文件上传" : "File Upload";
        public string EditTabText => _isChinese ? "编辑 Markdown" : "Edit Markdown";
        public string SelectedFileText => _isChinese ? "已选择文件:" : "Selected File:";
        public string EditContentText => _isChinese ? "编辑内容:" : "Edit Content:";
        public string WindowTitle => _isChinese ? "Markdown转PDF工具" : "Markdown to PDF Converter";
        public string PreviewText => $"## {(_isChinese ? "预览" : "Preview")}";

        public string Background
        {
            get => _background;
            set => this.RaiseAndSetIfChanged(ref _background, value);
        }

        public string Foreground
        {
            get => _foreground;
            set => this.RaiseAndSetIfChanged(ref _foreground, value);
        }

        public string BorderBrush
        {
            get => _borderBrush;
            set => this.RaiseAndSetIfChanged(ref _borderBrush, value);
        }

        public string TextBoxBackground
        {
            get => _textBoxBackground;
            set => this.RaiseAndSetIfChanged(ref _textBoxBackground, value);
        }

        public string TextBoxForeground
        {
            get => _textBoxForeground;
            set => this.RaiseAndSetIfChanged(ref _textBoxForeground, value);
        }

        public double FontSize
        {
            get => _fontSize;
            set => this.RaiseAndSetIfChanged(ref _fontSize, value);
        }

        private string _selectedFilePath;
        public string SelectedFilePath
        {
            get => _selectedFilePath;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedFilePath, value);
                this.RaisePropertyChanged(nameof(CanConvert));
            }
        }

        private string _markdownText;
        public string MarkdownText
        {
            get => _markdownText;
            set
            {
                this.RaiseAndSetIfChanged(ref _markdownText, value);
                this.RaisePropertyChanged(nameof(CanConvert));
            }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
        }

        public ReactiveCommand<Unit, Unit> UploadFileCommand { get; }
        public ReactiveCommand<Unit, Unit> ConvertToPdfCommand { get; }

        public bool CanConvert => !string.IsNullOrEmpty(SelectedFilePath) || !string.IsNullOrWhiteSpace(MarkdownText);

        private readonly MarkdownToPdfService _converterService;

        public MainViewModel()
        {
            _converterService = new MarkdownToPdfService();

            UploadFileCommand = ReactiveCommand.CreateFromTask(UploadFileAsync);

            ConvertToPdfCommand = ReactiveCommand.CreateFromTask(ConvertToPdfAsync,
                this.WhenAnyValue(x => x.SelectedFilePath, x => x.MarkdownText,
                    (filePath, markdown) => !string.IsNullOrEmpty(filePath) || !string.IsNullOrWhiteSpace(markdown)));

            SwitchLanguageCommand = ReactiveCommand.Create(() =>
            {
                _isChinese = !_isChinese;
                this.RaisePropertyChanged(nameof(LanguageButtonText));
                this.RaisePropertyChanged(nameof(UploadButtonText));
                this.RaisePropertyChanged(nameof(ConvertButtonText));
                this.RaisePropertyChanged(nameof(HelpButtonText));
                this.RaisePropertyChanged(nameof(FileTabText));
                this.RaisePropertyChanged(nameof(EditTabText));
                this.RaisePropertyChanged(nameof(SelectedFileText));
                this.RaisePropertyChanged(nameof(EditContentText));
                this.RaisePropertyChanged(nameof(WindowTitle));
                this.RaisePropertyChanged(nameof(PreviewText));
            });

            SwitchThemeCommand = ReactiveCommand.Create(SwitchTheme);

            ApplyTheme(_theme);
        }

        private void SwitchTheme()
        {
            _theme = _theme switch
            {
                "Gray" => "Dark",
                "Dark" => "Light",
                _ => "Gray"
            };
            ApplyTheme(_theme);
        }

        private void ApplyTheme(string theme)
        {
            switch (theme)
            {
                case "Dark":
                    Background = "#1E1E1E";
                    Foreground = "#D0D0D0";
                    BorderBrush = "#444444";
                    TextBoxBackground = "#333333";
                    TextBoxForeground = "#FFFFFF";
                    break;
                case "Light":
                    Background = "#FFFFFF";
                    Foreground = "#D0D0D0";
                    BorderBrush = "#CCCCCC";
                    TextBoxBackground = "#FFFFFF";
                    TextBoxForeground = "#000000";
                    break;
                default:
                    Background = "#2E2E2E";
                    Foreground = "#D0D0D0";
                    BorderBrush = "#444444";
                    TextBoxBackground = "#333333";
                    TextBoxForeground = "#D0D0D0";
                    break;
            }

            // 确保所有相关属性都更新
            this.RaisePropertyChanged(nameof(Background));
            this.RaisePropertyChanged(nameof(Foreground));
            this.RaisePropertyChanged(nameof(BorderBrush));
            this.RaisePropertyChanged(nameof(TextBoxBackground));
            this.RaisePropertyChanged(nameof(TextBoxForeground));
        }

        private async Task UploadFileAsync()
        {
            var window = (Application.Current.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow;

            if (window == null)
            {
                StatusMessage = "无法获取主窗口";
                return;
            }

            var files = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "选择 Markdown 文件",
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("Markdown Files")
                    {
                        Patterns = new[] { "*.md", "*.markdown" }
                    },
                    new FilePickerFileType("All Files")
                    {
                        Patterns = new[] { "*" }
                    }
                },
                AllowMultiple = false
            });

            if (files.Count > 0)
            {
                var file = files[0];
                SelectedFilePath = file.Path.LocalPath;
                using var stream = await file.OpenReadAsync();
                using var reader = new StreamReader(stream);
                MarkdownText = await reader.ReadToEndAsync();
                StatusMessage = "文件已加载";
            }
        }

        private async Task ConvertToPdfAsync()
        {
            try
            {
                string markdownContent = MarkdownText;

                if (string.IsNullOrEmpty(markdownContent))
                {
                    StatusMessage = "没有 Markdown 内容可转换";
                    return;
                }

                var window = (Application.Current.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow;

                if (window == null)
                {
                    StatusMessage = "无法获取主窗口";
                    return;
                }

                var file = await window.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "保存 PDF 文件",
                    FileTypeChoices = new[]
                    {
                        new FilePickerFileType("PDF Files")
                        {
                            Patterns = new[] { "*.pdf" }
                        }
                    },
                    DefaultExtension = "pdf"
                });

                if (file == null)
                {
                    StatusMessage = "保存已取消";
                    return;
                }

                using var stream = await file.OpenWriteAsync();
                _converterService.ConvertMarkdownToPdf(markdownContent, stream);

                StatusMessage = $"转换成功！文件已保存到 {file.Path.LocalPath}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"转换失败: {ex.Message}";
            }
        }
    }
}
