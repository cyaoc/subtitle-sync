using System;

namespace subtitle_sync.Models;

/// <summary>
/// 字幕条目，包含时间信息和文本内容
/// </summary>
public class SubtitleEntry
{
    /// <summary>
    /// 条目序号
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public TimeSpan StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public TimeSpan EndTime { get; set; }

    /// <summary>
    /// 字幕文本内容
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// 样式信息（用于高级格式如ASS）
    /// </summary>
    public string? Style { get; set; }

    /// <summary>
    /// 持续时间
    /// </summary>
    public TimeSpan Duration => EndTime - StartTime;

    public SubtitleEntry()
    {
    }

    public SubtitleEntry(int index, TimeSpan startTime, TimeSpan endTime, string text, string? style = null)
    {
        Index = index;
        StartTime = startTime;
        EndTime = endTime;
        Text = text;
        Style = style;
    }

    public override string ToString()
    {
        return $"[{Index}] {StartTime:hh\\:mm\\:ss\\.fff} --> {EndTime:hh\\:mm\\:ss\\.fff}: {Text}";
    }
} 