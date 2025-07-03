using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using subtitle_sync.ViewModels;

namespace subtitle_sync;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        // 设置拖拽事件处理
        var dropZone = this.FindControl<Border>("DropZone");
        if (dropZone != null)
        {
            dropZone.AddHandler(DragDrop.DropEvent, OnDrop);
            dropZone.AddHandler(DragDrop.DragOverEvent, OnDragOver);
            dropZone.AddHandler(DragDrop.DragEnterEvent, OnDragEnter);
            dropZone.AddHandler(DragDrop.DragLeaveEvent, OnDragLeave);
        }
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        // 检查是否包含文件
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

    private void OnDragEnter(object? sender, DragEventArgs e)
    {
        // 拖拽进入时的视觉反馈
        if (sender is Border border && e.Data.Contains(DataFormats.Files))
        {
            border.Background = Avalonia.Media.Brushes.LightBlue;
            border.BorderBrush = Avalonia.Media.Brushes.DodgerBlue;
        }
    }

    private void OnDragLeave(object? sender, DragEventArgs e)
    {
        // 拖拽离开时恢复样式
        if (sender is Border border)
        {
            border.Background = Avalonia.Media.Brush.Parse("#FFF8F8F8");
            border.BorderBrush = Avalonia.Media.Brush.Parse("#FFDDDDDD");
        }
    }

    private async void OnDrop(object? sender, DragEventArgs e)
    {
        // 恢复样式
        if (sender is Border border)
        {
            border.Background = Avalonia.Media.Brush.Parse("#FFF8F8F8");
            border.BorderBrush = Avalonia.Media.Brush.Parse("#FFDDDDDD");
        }

        // 处理文件拖放
        if (e.Data.Contains(DataFormats.Files))
        {
            var files = e.Data.GetFiles();
            if (files != null && DataContext is MainWindowViewModel viewModel)
            {
                await viewModel.HandleDroppedFilesAsync(files);
            }
        }
    }
}