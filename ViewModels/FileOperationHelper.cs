using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using subtitle_sync.Models;

namespace subtitle_sync.ViewModels;

/// <summary>
/// 文件操作帮助类，处理文件选择和保存逻辑
/// </summary>
public static class FileOperationHelper
{
    /// <summary>
    /// 打开文件选择对话框
    /// </summary>
    public static async Task<string?> OpenFileDialogAsync()
    {
        var topLevel = GetTopLevel();
        if (topLevel?.StorageProvider == null) return null;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "选择字幕文件",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("字幕文件")
                {
                    Patterns = new[] { "*.srt", "*.vtt", "*.ass", "*.ssa", "*.sub" }
                },
                FilePickerFileTypes.All
            }
        });

        return files?.Count > 0 ? files[0].Path.LocalPath : null;
    }

    /// <summary>
    /// 保存文件对话框
    /// </summary>
    public static async Task<string?> SaveFileDialogAsync(string defaultName, SubtitleFormat format)
    {
        var topLevel = GetTopLevel();
        if (topLevel?.StorageProvider == null) return null;

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "保存转换后的字幕文件",
            SuggestedFileName = GetSuggestedFileName(defaultName, format),
            FileTypeChoices = new[]
            {
                new FilePickerFileType(format.GetDisplayName())
                {
                    Patterns = new[] { $"*{format.GetFileExtension()}" }
                }
            }
        });

        return file?.Path.LocalPath;
    }

    /// <summary>
    /// 生成建议的文件名
    /// </summary>
    public static string GetSuggestedFileName(string originalPath, SubtitleFormat targetFormat)
    {
        if (string.IsNullOrEmpty(originalPath))
            return $"converted{targetFormat.GetFileExtension()}";

        var nameWithoutExtension = Path.GetFileNameWithoutExtension(originalPath);
        return $"{nameWithoutExtension}_converted{targetFormat.GetFileExtension()}";
    }

    /// <summary>
    /// 获取顶级窗口
    /// </summary>
    private static TopLevel? GetTopLevel()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return TopLevel.GetTopLevel(desktop.MainWindow);
        }
        return null;
    }
} 