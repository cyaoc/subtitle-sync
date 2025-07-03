using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace subtitle_sync.Models;

/// <summary>
/// 字幕文件，包含所有字幕条目和文件信息
/// </summary>
public class SubtitleFile
{
    /// <summary>
    /// 文件路径
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// 文件名
    /// </summary>
    public string FileName => Path.GetFileName(FilePath);

    /// <summary>
    /// 字幕格式
    /// </summary>
    public SubtitleFormat Format { get; set; } = SubtitleFormat.Unknown;

    /// <summary>
    /// 编码格式
    /// </summary>
    public string Encoding { get; set; } = "UTF-8";

    /// <summary>
    /// 字幕条目集合
    /// </summary>
    public ObservableCollection<SubtitleEntry> Entries { get; set; } = new();

    /// <summary>
    /// 文件标题/元数据
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// 样式定义（用于ASS/SSA格式）
    /// </summary>
    public Dictionary<string, string> Styles { get; set; } = new();

    /// <summary>
    /// 字幕条目数量
    /// </summary>
    public int Count => Entries.Count;

    /// <summary>
    /// 总时长
    /// </summary>
    public TimeSpan TotalDuration
    {
        get
        {
            if (Entries.Count == 0) return TimeSpan.Zero;
            return Entries.Max(e => e.EndTime);
        }
    }

    public SubtitleFile()
    {
    }

    public SubtitleFile(string filePath, SubtitleFormat format)
    {
        FilePath = filePath;
        Format = format;
    }

    /// <summary>
    /// 添加字幕条目
    /// </summary>
    public void AddEntry(SubtitleEntry entry)
    {
        Entries.Add(entry);
    }

    /// <summary>
    /// 按时间排序字幕条目
    /// </summary>
    public void SortByTime()
    {
        var sorted = Entries.OrderBy(e => e.StartTime).ToList();
        Entries.Clear();
        for (int i = 0; i < sorted.Count; i++)
        {
            sorted[i].Index = i + 1;
            Entries.Add(sorted[i]);
        }
    }

    /// <summary>
    /// 验证字幕文件有效性
    /// </summary>
    public bool IsValid()
    {
        return Entries.Count > 0 && 
               Entries.All(e => e.StartTime <= e.EndTime) &&
               Format != SubtitleFormat.Unknown;
    }

    public override string ToString()
    {
        return $"{FileName} ({Format.GetDisplayName()}) - {Count} 条目";
    }
} 