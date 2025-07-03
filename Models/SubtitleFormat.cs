namespace subtitle_sync.Models;

public enum SubtitleFormat
{
    Unknown,
    SRT,    // SubRip 格式
    VTT,    // WebVTT 格式
    ASS     // Advanced SSA 格式
}

public static class SubtitleFormatExtensions
{
    public static string GetFileExtension(this SubtitleFormat format)
    {
        return format switch
        {
            SubtitleFormat.SRT => ".srt",
            SubtitleFormat.VTT => ".vtt",
            SubtitleFormat.ASS => ".ass",
            _ => ""
        };
    }

    public static string GetDisplayName(this SubtitleFormat format)
    {
        return format switch
        {
            SubtitleFormat.SRT => "SRT (SubRip)",
            SubtitleFormat.VTT => "VTT (WebVTT)",
            SubtitleFormat.ASS => "ASS (Advanced SSA)",
            _ => "Unknown Format"
        };
    }

    public static SubtitleFormat FromFileExtension(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".srt" => SubtitleFormat.SRT,
            ".vtt" => SubtitleFormat.VTT,
            ".ass" => SubtitleFormat.ASS,
            _ => SubtitleFormat.Unknown
        };
    }
} 