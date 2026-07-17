using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace MarkdownToPdfConverter.Services
{
    /// <summary>
    /// Provides file picker dialogs via the main window's storage provider.
    /// </summary>
    public static class WindowService
    {
        public static Window? MainWindow { get; set; }

        private static IStorageProvider? StorageProvider => MainWindow?.StorageProvider;

        /// <summary>
        /// Opens a file picker dialog for selecting one or more files.
        /// </summary>
        public static async Task<IReadOnlyList<IStorageFile>> OpenFilePickerAsync(FilePickerOpenOptions options)
        {
            var sp = StorageProvider;
            return sp == null ? [] : await sp.OpenFilePickerAsync(options);
        }

        /// <summary>
        /// Opens a save file dialog and returns the chosen file path.
        /// </summary>
        public static async Task<IStorageFile?> SaveFilePickerAsync(FilePickerSaveOptions options)
        {
            var sp = StorageProvider;
            return sp == null ? null : await sp.SaveFilePickerAsync(options);
        }
    }
}
