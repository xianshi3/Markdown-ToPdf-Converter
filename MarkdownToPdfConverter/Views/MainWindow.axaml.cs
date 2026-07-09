using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ReactiveUI;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using MarkdownToPdfConverter.Services;
using System.IO;
using Avalonia.Platform.Storage;
using MarkdownToPdfConverter.ViewModels;

namespace MarkdownToPdfConverter.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            AddHandler(DragDrop.DropEvent, OnDrop);
            AddHandler(DragDrop.DragOverEvent, OnDragOver);
        }

        private void OnDragOver(object? sender, DragEventArgs e)
        {
            if (e.Data.Contains(DataFormats.Files))
            {
                e.DragEffects = DragDropEffects.Copy;
            }
            else
            {
                e.DragEffects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private async void OnDrop(object? sender, DragEventArgs e)
        {
            if (e.Data.Contains(DataFormats.Files))
            {
                var files = e.Data.GetFiles();
                if (files != null)
                {
                    var fileArray = files.ToArray();
                    if (fileArray.Length > 0)
                    {
                        var file = fileArray[0];
                        var path = file.Path.LocalPath;

                        if (path.EndsWith(".md", StringComparison.OrdinalIgnoreCase) || 
                            path.EndsWith(".markdown", StringComparison.OrdinalIgnoreCase))
                        {
                            if (DataContext is MainViewModel vm)
                            {
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
                        else
                        {
                            if (DataContext is MainViewModel vm)
                            {
                                vm.StatusMessage = vm.DropMdHintText;
                            }
                        }
                    }
                }
            }
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                var ctrl = e.KeyModifiers.HasFlag(KeyModifiers.Control);
                if (!ctrl) return;

                var shift = e.KeyModifiers.HasFlag(KeyModifiers.Shift);

                switch (e.Key)
                {
                    case Key.N:
                        vm.NewFileCommand.Execute().Subscribe();
                        e.Handled = true;
                        break;
                    case Key.O:
                        vm.OpenFileCommand.Execute().Subscribe();
                        e.Handled = true;
                        break;
                    case Key.S:
                        if (shift)
                            vm.SaveAsCommand.Execute().Subscribe();
                        else
                            vm.SaveFileCommand.Execute().Subscribe();
                        e.Handled = true;
                        break;
                    case Key.Z:
                        vm.UndoCommand.Execute().Subscribe();
                        e.Handled = true;
                        break;
                    case Key.Y:
                        vm.RedoCommand.Execute().Subscribe();
                        e.Handled = true;
                        break;
                    case Key.F:
                        vm.FindCommand.Execute().Subscribe();
                        e.Handled = true;
                        break;
                    case Key.L:
                        vm.SwitchLanguageCommand.Execute().Subscribe();
                        e.Handled = true;
                        break;
                    case Key.T:
                        vm.SwitchThemeCommand.Execute().Subscribe();
                        e.Handled = true;
                        break;
                }
            }
        }
    }
}