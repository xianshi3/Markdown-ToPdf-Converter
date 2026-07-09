using System;
using Avalonia.Controls;
using Avalonia.Input;
using ReactiveUI;
using MarkdownToPdfConverter.Services;
using MarkdownToPdfConverter.ViewModels;

namespace MarkdownToPdfConverter.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            RootGrid.AddHandler(DragDrop.DragOverEvent, OnRootDragOver);
            RootGrid.AddHandler(DragDrop.DropEvent, OnRootDrop);
        }

        private void OnRootDragOver(object? sender, DragEventArgs e)
        {
            e.DragEffects = DragDropEffects.Copy;
        }

        private async void OnRootDrop(object? sender, DragEventArgs e)
        {
            if (DataContext is MainViewModel vm)
                await FileDropHelper.HandleFileDrop(e, vm);
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
                    case Key.H:
                        vm.ReplaceCommand.Execute().Subscribe();
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
                    case Key.B:
                        vm.InsertBoldCommand.Execute().Subscribe();
                        e.Handled = true;
                        break;
                    case Key.I:
                        vm.InsertItalicCommand.Execute().Subscribe();
                        e.Handled = true;
                        break;
                    case Key.P:
                        vm.TogglePreviewCommand.Execute().Subscribe();
                        e.Handled = true;
                        break;
                    case Key.OemPlus:
                        if (shift)
                            vm.ZoomInCommand.Execute().Subscribe();
                        e.Handled = true;
                        break;
                    case Key.OemMinus:
                        vm.ZoomOutCommand.Execute().Subscribe();
                        e.Handled = true;
                        break;
                    case Key.D0:
                        vm.ZoomResetCommand.Execute().Subscribe();
                        e.Handled = true;
                        break;
                }
            }
        }
    }
}
