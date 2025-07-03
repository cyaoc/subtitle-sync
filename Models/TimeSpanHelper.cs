using System;

namespace subtitle_sync.Models;

/// <summary>
/// 时间解析帮助类，统一处理各种字幕格式的时间解析
/// </summary>
public static class TimeSpanHelper
{
    /// <summary>
    /// 解析SRT格式时间：HH:MM:SS,mmm
    /// </summary>
    public static TimeSpan ParseSrtTime(string hours, string minutes, string seconds, string milliseconds)
    {
        return new TimeSpan(0, int.Parse(hours), int.Parse(minutes), int.Parse(seconds), int.Parse(milliseconds));
    }

    /// <summary>
    /// 解析VTT格式时间：HH:MM:SS.mmm
    /// </summary>
    public static TimeSpan ParseVttTime(string hours, string minutes, string seconds, string milliseconds)
    {
        return new TimeSpan(0, int.Parse(hours), int.Parse(minutes), int.Parse(seconds), int.Parse(milliseconds));
    }

    /// <summary>
    /// 解析ASS格式时间：H:MM:SS.cc (centiseconds)
    /// </summary>
    public static TimeSpan ParseAssTime(string hours, string minutes, string seconds, string centiseconds)
    {
        return new TimeSpan(0, int.Parse(hours), int.Parse(minutes), int.Parse(seconds), int.Parse(centiseconds) * 10);
    }

    /// <summary>
    /// 格式化为SRT时间格式
    /// </summary>
    public static string FormatSrtTime(TimeSpan time)
    {
        return $"{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2},{time.Milliseconds:D3}";
    }

    /// <summary>
    /// 格式化为VTT时间格式
    /// </summary>
    public static string FormatVttTime(TimeSpan time)
    {
        return $"{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2}.{time.Milliseconds:D3}";
    }

    /// <summary>
    /// 格式化为ASS时间格式
    /// </summary>
    public static string FormatAssTime(TimeSpan time)
    {
        return $"{time.Hours}:{time.Minutes:D2}:{time.Seconds:D2}.{time.Milliseconds / 10:D2}";
    }
} 