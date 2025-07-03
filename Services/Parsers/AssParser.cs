using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using subtitle_sync.Models;

namespace subtitle_sync.Services.Parsers;

/// <summary>
/// ASS (Advanced SSA) 格式解析器
/// </summary>
public class AssParser : ISubtitleParser
{
    private static readonly Regex AssTimeRegex = new Regex(
        @"(\d):(\d{2}):(\d{2})\.(\d{2})",
        RegexOptions.Compiled);

    private static readonly Regex DialogueRegex = new Regex(
        @"^Dialogue:\s*\d+,(\d:\d{2}:\d{2}\.\d{2}),(\d:\d{2}:\d{2}\.\d{2}),([^,]*),([^,]*),([^,]*),([^,]*),([^,]*),([^,]*),(.*)$",
        RegexOptions.Compiled);

            public SubtitleFormat SupportedFormat => SubtitleFormat.ASS;

    public async Task<bool> CanParseAsync(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            return false;

        try
        {
            var content = await File.ReadAllTextAsync(filePath);
            return await CanParseFromContentAsync(content);
        }
        catch
        {
            return false;
        }
    }

    public Task<bool> CanParseFromContentAsync(string content)
    {
        if (string.IsNullOrEmpty(content))
            return Task.FromResult(false);

        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                         .Select(l => l.Trim())
                         .Where(l => !string.IsNullOrEmpty(l))
                         .ToArray();

        if (lines.Length < 2)
            return Task.FromResult(false);

        // Check for ASS file markers
        bool hasAssHeader = lines.Any(line => line.StartsWith("[Script Info]", StringComparison.OrdinalIgnoreCase) ||
                                            line.StartsWith("[V4+ Styles]", StringComparison.OrdinalIgnoreCase) ||
                                            line.StartsWith("[V4 Styles]", StringComparison.OrdinalIgnoreCase) ||
                                            line.StartsWith("[Events]", StringComparison.OrdinalIgnoreCase));

        // Check for dialogue lines
        bool hasDialogue = lines.Any(line => line.StartsWith("Dialogue:", StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(hasAssHeader && hasDialogue);
    }

    public async Task<SubtitleFile> ParseAsync(string filePath)
    {
        if (!await CanParseAsync(filePath))
            throw new ArgumentException("File is not a valid ASS file", nameof(filePath));

        var content = await File.ReadAllTextAsync(filePath);
        return await ParseFromContentAsync(content, filePath);
    }

    public Task<SubtitleFile> ParseFromContentAsync(string content, string filename = "test.subtitle")
    {
        return Task.Run(() =>
        {
            var entries = new List<SubtitleEntry>();
            var lines = content.Split('\n').Select(l => l.Trim()).ToArray();

            int index = 1;
            foreach (var line in lines)
            {
                if (!line.StartsWith("Dialogue:", StringComparison.OrdinalIgnoreCase))
                    continue;

                var match = DialogueRegex.Match(line);
                if (!match.Success) continue;

                var startTimeStr = match.Groups[1].Value;
                var endTimeStr = match.Groups[2].Value;
                var text = match.Groups[9].Value;

                // Parse times
                var startTimeMatch = AssTimeRegex.Match(startTimeStr);
                var endTimeMatch = AssTimeRegex.Match(endTimeStr);

                if (!startTimeMatch.Success || !endTimeMatch.Success) continue;

                var startTime = ParseAssTime(startTimeMatch.Groups[1].Value, startTimeMatch.Groups[2].Value,
                                           startTimeMatch.Groups[3].Value, startTimeMatch.Groups[4].Value);
                var endTime = ParseAssTime(endTimeMatch.Groups[1].Value, endTimeMatch.Groups[2].Value,
                                         endTimeMatch.Groups[3].Value, endTimeMatch.Groups[4].Value);

                // Clean up text (remove ASS tags)
                text = RemoveAssTags(text);

                entries.Add(new SubtitleEntry
                {
                    Index = index++,
                    StartTime = startTime,
                    EndTime = endTime,
                    Text = text
                });
            }

            return new SubtitleFile
            {
                Entries = new ObservableCollection<SubtitleEntry>(entries),
                Format = SubtitleFormat.ASS
            };
        });
    }

    public Task<string> GenerateAsync(SubtitleFile subtitleFile)
    {
        return Task.Run(() =>
        {
            var sb = new StringBuilder();
            
            // ASS文件头
            sb.AppendLine("[Script Info]");
            sb.AppendLine($"Title: {subtitleFile.Title ?? "Converted Subtitle"}");
            sb.AppendLine("ScriptType: v4.00+");
            sb.AppendLine();

            sb.AppendLine("[V4+ Styles]");
            sb.AppendLine("Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding");
            sb.AppendLine("Style: Default,Arial,20,&H00FFFFFF,&H000000FF,&H00000000,&H80000000,0,0,0,0,100,100,0,0,1,2,0,2,10,10,10,1");
            sb.AppendLine();

            sb.AppendLine("[Events]");
            sb.AppendLine("Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text");

            foreach (var entry in subtitleFile.Entries.OrderBy(e => e.StartTime))
            {
                var startTime = FormatAssTime(entry.StartTime);
                var endTime = FormatAssTime(entry.EndTime);
                var text = entry.Text.Replace("\n", "\\N"); // ASS使用\N作为换行符
                
                sb.AppendLine($"Dialogue: 0,{startTime},{endTime},Default,,0,0,0,,{text}");
            }

            return sb.ToString();
        });
    }

    private TimeSpan ParseAssTime(string hours, string minutes, string seconds, string centiseconds)
    {
        return new TimeSpan(0, int.Parse(hours), int.Parse(minutes), int.Parse(seconds), int.Parse(centiseconds) * 10);
    }

    private string FormatAssTime(TimeSpan time)
    {
        return $"{time.Hours}:{time.Minutes:D2}:{time.Seconds:D2}.{time.Milliseconds / 10:D2}";
    }

    private string RemoveAssTags(string text)
    {
        // Remove ASS style tags like {\tag}
        return Regex.Replace(text, @"\{[^}]*\}", "");
    }
} 