using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using ReactiveUI;
using MarkdownToPdfConverter.Services;
using MarkdownToPdfConverter.ViewModels;

namespace MarkdownToPdfConverter.Views
{
    /// <summary>The main application window hosting all components and handling global input.</summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            RootGrid.AddHandler(DragDrop.DragOverEvent, OnRootDragOver);
            RootGrid.AddHandler(DragDrop.DropEvent, OnRootDrop);
            Closing += OnClosing;
        }

        private void OnRootDragOver(object? sender, DragEventArgs e)
        {
            e.DragEffects = DragDropEffects.Copy;
        }

        private async void OnRootDrop(object? sender, DragEventArgs e)
        {
            try
            {
                if (DataContext is MainViewModel vm)
                    await FileDropHelper.HandleFileDrop(e, vm);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MainWindow drop failed: {ex.Message}");
            }
        }

        private async void OnClosing(object? sender, WindowClosingEventArgs e)
        {
            if (DataContext is MainViewModel vm && vm.HasUnsavedChanges)
            {
                e.Cancel = true;
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    var message = LocalizationService.Instance.GetString("confirm_close");
                    var result = await MainViewModel.ShowConfirmDialogAsync(message);
                    if (result)
                        Close();
                });
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
                        _ = vm.NewFileCommand.Execute().Subscribe();
                        e.Handled = true;
                        break;
                    case Key.O:
                        _ = vm.OpenFileCommand.Execute().Subscribe();
                        e.Handled = true;
                        break;
                    case Key.S:
                        if (shift)
                            _ = vm.SaveAsCommand.Execute().Subscribe();
                        else
                            _ = vm.SaveFileCommand.Execute().Subscribe();
                        e.Handled = true;
                        break;
                    case Key.Z:
                        _ = vm.UndoCommand.Execute().Subscribe();
                        e.Handled = true;
                        break;
                    case Key.Y:
                        _ = vm.RedoCommand.Execute().Subscribe();
                        e.Handled = true;
                        break;
                    case Key.F:
                        _ = vm.FindCommand.Execute().Subscribe();
                        e.Handled = true;
                        break;
                    case Key.H:
                        _ = vm.ReplaceCommand.Execute().Subscribe();
                        e.Handled = true;
                        break;
                    case Key.L:
                        _ = vm.SwitchLanguageCommand.Execute().Subscribe();
                        e.Handled = true;
                        break;
                    case Key.T:
                        _ = vm.SwitchThemeCommand.Execute().Subscribe();
                        e.Handled = true;
                        break;
                    case Key.B:
                        _ = vm.InsertBoldCommand.Execute().Subscribe();
                        e.Handled = true;
                        break;
                    case Key.I:
                        _ = vm.InsertItalicCommand.Execute().Subscribe();
                        e.Handled = true;
                        break;
                    case Key.P:
                        _ = vm.TogglePreviewCommand.Execute().Subscribe();
                        e.Handled = true;
                        break;
                    case Key.OemPlus:
                        if (shift)
                            _ = vm.ZoomInCommand.Execute().Subscribe();
                        e.Handled = true;
                        break;
                    case Key.OemMinus:
                        _ = vm.ZoomOutCommand.Execute().Subscribe();
                        e.Handled = true;
                        break;
                    case Key.D0:
                        _ = vm.ZoomResetCommand.Execute().Subscribe();
                        e.Handled = true;
                        break;
                }
            }
        }
    }
}
