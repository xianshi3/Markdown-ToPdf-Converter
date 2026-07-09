using Avalonia.Controls;
using Avalonia.Input;
using MarkdownToPdfConverter.Services;
using MarkdownToPdfConverter.ViewModels;

namespace MarkdownToPdfConverter.Views.Components;

public partial class EditorComponent : UserControl
{
    public EditorComponent()
    {
        InitializeComponent();

        DropFileTab.AddHandler(DragDrop.DragOverEvent, OnFileTabDragOver);
        DropFileTab.AddHandler(DragDrop.DragLeaveEvent, OnFileTabDragLeave);
        DropFileTab.AddHandler(DragDrop.DropEvent, OnFileTabDrop);
    }

    private void OnFileTabDragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = DragDropEffects.Copy;
        DropFileTab.Classes.Add("dragover");
    }

    private void OnFileTabDragLeave(object? sender, DragEventArgs e)
    {
        DropFileTab.Classes.Remove("dragover");
    }

    private async void OnFileTabDrop(object? sender, DragEventArgs e)
    {
        DropFileTab.Classes.Remove("dragover");

        if (DataContext is MainViewModel vm)
            await FileDropHelper.HandleFileDrop(e, vm);
    }
}
