using System.Text.RegularExpressions;

namespace subtitle_sync.Models;

/// <summary>
/// 预编译的正则表达式模式，提升解析性能
/// </summary>
public static class RegexPatterns
{
    /// <summary>
    /// SRT时间格式：00:00:01,000 --> 00:00:03,000
    /// </summary>
    public static readonly Regex SrtTimeRegex = new(
        @"(\d{2}):(\d{2}):(\d{2}),(\d{3}) --> (\d{2}):(\d{2}):(\d{2}),(\d{3})",
        RegexOptions.Compiled);

    /// <summary>
    /// SRT索引格式：纯数字行
    /// </summary>
    public static readonly Regex SrtIndexRegex = new(
        @"^\d+$",
        RegexOptions.Compiled);

    /// <summary>
    /// VTT时间格式：00:00:01.000 --> 00:00:03.000
    /// </summary>
    public static readonly Regex VttTimeRegex = new(
        @"(\d{2}):(\d{2}):(\d{2})\.(\d{3}) --> (\d{2}):(\d{2}):(\d{2})\.(\d{3})",
        RegexOptions.Compiled);

    /// <summary>
    /// ASS时间格式：1:00:00.00
    /// </summary>
    public static readonly Regex AssTimeRegex = new(
        @"(\d):(\d{2}):(\d{2})\.(\d{2})",
        RegexOptions.Compiled);

    /// <summary>
    /// ASS对话行格式
    /// </summary>
    public static readonly Regex DialogueRegex = new(
        @"^Dialogue:\s*\d+,(\d:\d{2}:\d{2}\.\d{2}),(\d:\d{2}:\d{2}\.\d{2}),([^,]*),([^,]*),([^,]*),([^,]*),([^,]*),([^,]*),(.*)$",
        RegexOptions.Compiled);

    /// <summary>
    /// ASS样式标签清除：{\tag}
    /// </summary>
    public static readonly Regex AssTagRemovalRegex = new(
        @"\{[^}]*\}",
        RegexOptions.Compiled);
} 