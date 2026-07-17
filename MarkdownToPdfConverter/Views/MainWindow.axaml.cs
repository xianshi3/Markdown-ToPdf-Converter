using System;
using Avalonia.Controls;
using Avalonia.Input;
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

            // Enable file drag-and-drop onto the main window
            RootGrid.AddHandler(DragDrop.DragOverEvent, OnRootDragOver);
            RootGrid.AddHandler(DragDrop.DropEvent, OnRootDrop);
        }

        /// <summary>Indicates a copy operation when files are dragged over the window.</summary>
        private void OnRootDragOver(object? sender, DragEventArgs e)
        {
            e.DragEffects = DragDropEffects.Copy;
        }

        /// <summary>Handles files dropped onto the window by delegating to the view-model.</summary>
        private async void OnRootDrop(object? sender, DragEventArgs e)
        {
            if (DataContext is MainViewModel vm)
                await FileDropHelper.HandleFileDrop(e, vm);
        }

        /// <summary>Handles keyboard shortcuts for file operations, editing, and navigation.</summary>
        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                var ctrl = e.KeyModifiers.HasFlag(KeyModifiers.Control);
                if (!ctrl) return;

                var shift = e.KeyModifiers.HasFlag(KeyModifiers.Shift);

                switch (e.Key)
                {
                    // Ctrl+N: new file
                    case Key.N:
                        vm.NewFileCommand.Execute().Subscribe();
                        e.Handled = true;
                        break;
                    // Ctrl+O: open file
                    case Key.O:
                        vm.OpenFileCommand.Execute().Subscribe();
                        e.Handled = true;
                        break;
                    // Ctrl+S: save; Ctrl+Shift+S: save as
                    case Key.S:
                        if (shift)
                            vm.SaveAsCommand.Execute().Subscribe();
                        else
                            vm.SaveFileCommand.Execute().Subscribe();
                        e.Handled = true;
                        break;
                    // Ctrl+Z: undo
                    case Key.Z:
                        vm.UndoCommand.Execute().Subscribe();
                        e.Handled = true;
                        break;
                    // Ctrl+Y: redo
                    case Key.Y:
                        vm.RedoCommand.Execute().Subscribe();
                        e.Handled = true;
                        break;
                    // Ctrl+F: find
                    case Key.F:
                        vm.FindCommand.Execute().Subscribe();
                        e.Handled = true;
                        break;
                    // Ctrl+H: replace
                    case Key.H:
                        vm.ReplaceCommand.Execute().Subscribe();
                        e.Handled = true;
                        break;
                    // Ctrl+L: switch language
                    case Key.L:
                        vm.SwitchLanguageCommand.Execute().Subscribe();
                        e.Handled = true;
                        break;
                    // Ctrl+T: switch theme
                    case Key.T:
                        vm.SwitchThemeCommand.Execute().Subscribe();
                        e.Handled = true;
                        break;
                    // Ctrl+B: insert bold markers
                    case Key.B:
                        vm.InsertBoldCommand.Execute().Subscribe();
                        e.Handled = true;
                        break;
                    // Ctrl+I: insert italic markers
                    case Key.I:
                        vm.InsertItalicCommand.Execute().Subscribe();
                        e.Handled = true;
                        break;
                    // Ctrl+P: toggle preview panel
                    case Key.P:
                        vm.TogglePreviewCommand.Execute().Subscribe();
                        e.Handled = true;
                        break;
                    // Ctrl+Shift+=: zoom in
                    case Key.OemPlus:
                        if (shift)
                            vm.ZoomInCommand.Execute().Subscribe();
                        e.Handled = true;
                        break;
                    // Ctrl+-: zoom out
                    case Key.OemMinus:
                        vm.ZoomOutCommand.Execute().Subscribe();
                        e.Handled = true;
                        break;
                    // Ctrl+0: reset zoom
                    case Key.D0:
                        vm.ZoomResetCommand.Execute().Subscribe();
                        e.Handled = true;
                        break;
                }
            }
        }
    }
}
