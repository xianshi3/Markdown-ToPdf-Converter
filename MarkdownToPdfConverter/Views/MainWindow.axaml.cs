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
    /// <summary>
    /// 主视图模型，负责处理用户界面逻辑和文件转换操作
    /// </summary>
    public class MainViewModel : ReactiveObject
    {
        private bool _isChinese = true; // 默认中文

        // 添加语言切换命令
        public ReactiveCommand<Unit, Unit> SwitchLanguageCommand { get; }

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

        private string _selectedFilePath;
        /// <summary>
        /// 获取或设置用户选择的文件路径
        /// </summary>
        public string SelectedFilePath
        {
            get => _selectedFilePath;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedFilePath, value);
                // 手动触发 CanConvert 更新
                this.RaisePropertyChanged(nameof(CanConvert));
            }
        }

        private string _markdownText;
        /// <summary>
        /// 获取或设置用户输入的 Markdown 文本
        /// </summary>
        public string MarkdownText
        {
            get => _markdownText;
            set
            {
                this.RaiseAndSetIfChanged(ref _markdownText, value);
                // 手动触发 CanConvert 更新
                this.RaisePropertyChanged(nameof(CanConvert));
            }
        }

        private string _statusMessage;
        /// <summary>
        /// 获取或设置状态消息，用于向用户显示操作结果或错误信息
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
        }

        /// <summary>
        /// 获取上传文件的命令
        /// </summary>
        public ReactiveCommand<Unit, Unit> UploadFileCommand { get; }

        /// <summary>
        /// 获取将 Markdown 转换为 PDF 的命令
        /// </summary>
        public ReactiveCommand<Unit, Unit> ConvertToPdfCommand { get; }

        /// <summary>
        /// 判断是否可以进行 PDF 转换
        /// </summary>
        public bool CanConvert => !string.IsNullOrEmpty(SelectedFilePath) || !string.IsNullOrWhiteSpace(MarkdownText);

        private readonly MarkdownToPdfService _converterService;

        /// <summary>
        /// 初始化 <see cref="MainViewModel"/> 类的新实例
        /// </summary>
        public MainViewModel()
        {
            _converterService = new MarkdownToPdfService();

            // 上传文件命令
            UploadFileCommand = ReactiveCommand.CreateFromTask(UploadFileAsync);

            // 转换为 PDF 命令，当 SelectedFilePath 或 MarkdownText 不为空时启用
            ConvertToPdfCommand = ReactiveCommand.CreateFromTask(ConvertToPdfAsync,
                this.WhenAnyValue(x => x.SelectedFilePath, x => x.MarkdownText,
                    (filePath, markdown) => !string.IsNullOrEmpty(filePath) || !string.IsNullOrWhiteSpace(markdown)));

            // 添加语言切换命令
            SwitchLanguageCommand = ReactiveCommand.Create(() =>
            {
                _isChinese = !_isChinese;
                // 通知所有语言相关属性更新
                this.RaisePropertyChanged(nameof(LanguageButtonText));
                this.RaisePropertyChanged(nameof(UploadButtonText));
                this.RaisePropertyChanged(nameof(ConvertButtonText));
                this.RaisePropertyChanged(nameof(HelpButtonText));
                this.RaisePropertyChanged(nameof(FileTabText));
                this.RaisePropertyChanged(nameof(EditTabText));
                this.RaisePropertyChanged(nameof(SelectedFileText));
                this.RaisePropertyChanged(nameof(EditContentText));
                this.RaisePropertyChanged(nameof(WindowTitle));
            });
        }

        /// <summary>
        /// 上传文件并读取其内容
        /// </summary>
        /// <returns>表示异步操作的任务</returns>
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

        /// <summary>
        /// 将 Markdown 文本转换为 PDF 文件并保存
        /// </summary>
        /// <returns>表示异步操作的任务</returns>
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
