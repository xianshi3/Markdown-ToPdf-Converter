using Avalonia.Input;
using System;
using System.IO;
using System.Linq;
using MarkdownToPdfConverter.ViewModels;

namespace MarkdownToPdfConverter.Services
{
    /// <summary>
    /// Handles drag-and-drop of Markdown files onto the application window.
    /// </summary>
    internal static class FileDropHelper
    {
        /// <summary>
        /// Extracts a valid Markdown file path from the drag event data.
        /// Tries the modern IStorageProvider API first, then falls back to the legacy GetFileNames API.
        /// </summary>
        internal static string? TryGetFilePath(DragEventArgs e)
        {
            try
            {
                var files = e.Data.GetFiles();
                var file = files?.FirstOrDefault();
                if (file != null && IsValidMarkdownFile(file.Path.LocalPath))
                    return file.Path.LocalPath;
            }
            catch
            {
            }

            try
            {
#pragma warning disable CS0618
                var names = e.Data.GetFileNames();
#pragma warning restore CS0618
                var name = names?.FirstOrDefault();
                if (!string.IsNullOrEmpty(name) && IsValidMarkdownFile(name))
                    return name;
            }
            catch
            {
            }

            return null;
        }

        /// <summary>
        /// Checks whether the file path has a .md or .markdown extension.
        /// </summary>
        private static bool IsValidMarkdownFile(string path)
        {
            return path.EndsWith(".md", StringComparison.OrdinalIgnoreCase) ||
                   path.EndsWith(".markdown", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Handles a file drop event: loads the Markdown file content and updates the view model.
        /// </summary>
        internal static async System.Threading.Tasks.Task HandleFileDrop(DragEventArgs e, MainViewModel vm)
        {
            var path = TryGetFilePath(e);
            if (string.IsNullOrEmpty(path))
            {
                vm.StatusMessage = vm.DropMdHintText;
                return;
            }

            try
            {
                vm.SelectedFilePath = path;
                vm.MarkdownText = await File.ReadAllTextAsync(path);
                vm.StatusMessage = $"{vm.LoadFileStatusText} {Path.GetFileName(path)}";
            }
            catch (Exception ex)
            {
                vm.StatusMessage = $"{vm.LoadFailedText} {ex.Message}";
            }
        }
    }
}
