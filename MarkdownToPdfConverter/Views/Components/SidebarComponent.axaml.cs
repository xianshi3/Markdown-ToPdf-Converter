using Avalonia.Controls;
using Avalonia.Input;
using MarkdownToPdfConverter.Services;
using MarkdownToPdfConverter.ViewModels;

namespace MarkdownToPdfConverter.Views.Components;

public partial class SidebarComponent : UserControl
{
    public SidebarComponent()
    {
        InitializeComponent();

        DropSidebar.AddHandler(DragDrop.DragOverEvent, OnSidebarDragOver);
        DropSidebar.AddHandler(DragDrop.DragLeaveEvent, OnSidebarDragLeave);
        DropSidebar.AddHandler(DragDrop.DropEvent, OnSidebarDrop);
    }

    private void OnSidebarDragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = DragDropEffects.Copy;
        DropSidebar.Classes.Add("dragover");
    }

    private void OnSidebarDragLeave(object? sender, DragEventArgs e)
    {
        DropSidebar.Classes.Remove("dragover");
    }

    private async void OnSidebarDrop(object? sender, DragEventArgs e)
    {
        DropSidebar.Classes.Remove("dragover");

        if (DataContext is MainViewModel vm)
            await FileDropHelper.HandleFileDrop(e, vm);
    }
}
