using Avalonia.Controls;
using Avalonia.Input;
using MarkdownToPdfConverter.Services;
using MarkdownToPdfConverter.ViewModels;

namespace MarkdownToPdfConverter.Views.Components;

/// <summary>Markdown text editor panel with drag-and-drop file opening support.</summary>
public partial class EditorComponent : UserControl
{
    public EditorComponent()
    {
        InitializeComponent();

        // Enable file drag-and-drop on the file tab area
        DropFileTab.AddHandler(DragDrop.DragOverEvent, OnFileTabDragOver);
        DropFileTab.AddHandler(DragDrop.DragLeaveEvent, OnFileTabDragLeave);
        DropFileTab.AddHandler(DragDrop.DropEvent, OnFileTabDrop);
    }

    /// <summary>Shows a visual drag-over indicator when a file is dragged onto the tab area.</summary>
    private void OnFileTabDragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = DragDropEffects.Copy;
        DropFileTab.Classes.Add("dragover");
    }

    /// <summary>Removes the drag-over indicator when the dragged file leaves the tab area.</summary>
    private void OnFileTabDragLeave(object? sender, DragEventArgs e)
    {
        DropFileTab.Classes.Remove("dragover");
    }

    /// <summary>Handles a file dropped on the tab area and delegates to the view-model.</summary>
    private async void OnFileTabDrop(object? sender, DragEventArgs e)
    {
        DropFileTab.Classes.Remove("dragover");

        if (DataContext is MainViewModel vm)
            await FileDropHelper.HandleFileDrop(e, vm);
    }
}
