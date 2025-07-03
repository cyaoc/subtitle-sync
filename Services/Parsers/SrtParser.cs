using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using subtitle_sync.Models;

namespace subtitle_sync.Services.Parsers;

/// <summary>
/// SRT (SubRip) 格式解析器
/// </summary>
public class SrtParser : ISubtitleParser
{
    public SubtitleFormat SupportedFormat => SubtitleFormat.SRT;

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

        if (lines.Length < 3)
            return Task.FromResult(false);

        // Check if first line is a number (SRT index)
        if (!RegexPatterns.SrtIndexRegex.IsMatch(lines[0]))
            return Task.FromResult(false);

        // Check if second line contains SRT time format
        bool hasTimeFormat = RegexPatterns.SrtTimeRegex.IsMatch(lines[1]);
        
        return Task.FromResult(hasTimeFormat);
    }

    public async Task<SubtitleFile> ParseAsync(string filePath)
    {
        if (!await CanParseAsync(filePath))
            throw new ArgumentException("File is not a valid SRT file", nameof(filePath));

        var content = await File.ReadAllTextAsync(filePath);
        return await ParseFromContentAsync(content, filePath);
    }

    public Task<SubtitleFile> ParseFromContentAsync(string content, string filename = "test.subtitle")
    {
        return Task.Run(() =>
        {
            var entries = new List<SubtitleEntry>();
            var blocks = content.Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var block in blocks)
            {
                var lines = block.Split('\n').Select(l => l.Trim()).ToArray();
                if (lines.Length < 3) continue;

                if (!int.TryParse(lines[0], out int index)) continue;

                var timeMatch = RegexPatterns.SrtTimeRegex.Match(lines[1]);
                if (!timeMatch.Success) continue;

                var startTime = TimeSpanHelper.ParseSrtTime(timeMatch.Groups[1].Value, timeMatch.Groups[2].Value, 
                                                          timeMatch.Groups[3].Value, timeMatch.Groups[4].Value);
                var endTime = TimeSpanHelper.ParseSrtTime(timeMatch.Groups[5].Value, timeMatch.Groups[6].Value, 
                                                        timeMatch.Groups[7].Value, timeMatch.Groups[8].Value);

                var text = string.Join("\n", lines.Skip(2));

                entries.Add(new SubtitleEntry
                {
                    Index = index,
                    StartTime = startTime,
                    EndTime = endTime,
                    Text = text
                });
            }

            return new SubtitleFile
            {
                Entries = new ObservableCollection<SubtitleEntry>(entries),
                Format = SubtitleFormat.SRT
            };
        });
    }

    public Task<string> GenerateAsync(SubtitleFile subtitleFile)
    {
        return Task.Run(() =>
        {
            var srtContent = string.Join("\n\n", subtitleFile.Entries.Select((entry, i) =>
                $"{i + 1}\n{TimeSpanHelper.FormatSrtTime(entry.StartTime)} --> {TimeSpanHelper.FormatSrtTime(entry.EndTime)}\n{entry.Text}"));

            return srtContent;
        });
    }
} 