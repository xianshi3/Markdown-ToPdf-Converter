using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace MarkdownToPdfConverter.Services
{
    public static class WindowService
    {
        public static Window? MainWindow { get; set; }

        private static IStorageProvider? StorageProvider => MainWindow?.StorageProvider;

        public static async Task<IReadOnlyList<IStorageFile>> OpenFilePickerAsync(FilePickerOpenOptions options)
        {
            var sp = StorageProvider;
            return sp == null ? [] : await sp.OpenFilePickerAsync(options);
        }

        public static async Task<IStorageFile?> SaveFilePickerAsync(FilePickerSaveOptions options)
        {
            var sp = StorageProvider;
            return sp == null ? null : await sp.SaveFilePickerAsync(options);
        }
    }
}
