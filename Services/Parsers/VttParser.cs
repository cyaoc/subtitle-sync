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
/// VTT (WebVTT) 格式解析器
/// </summary>
public class VttParser : ISubtitleParser
{
    private static readonly Regex VttTimeRegex = new Regex(
        @"(\d{2}):(\d{2}):(\d{2})\.(\d{3}) --> (\d{2}):(\d{2}):(\d{2})\.(\d{3})",
        RegexOptions.Compiled);

    public SubtitleFormat SupportedFormat => SubtitleFormat.VTT;

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

        // Check if first line is WEBVTT
        if (!lines[0].StartsWith("WEBVTT", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(false);

        // Look for at least one time line
        bool hasTimeFormat = lines.Any(line => VttTimeRegex.IsMatch(line));
        
        return Task.FromResult(hasTimeFormat);
    }

    public async Task<SubtitleFile> ParseAsync(string filePath)
    {
        if (!await CanParseAsync(filePath))
            throw new ArgumentException("File is not a valid VTT file", nameof(filePath));

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
            for (int i = 0; i < lines.Length; i++)
            {
                var timeMatch = VttTimeRegex.Match(lines[i]);
                if (!timeMatch.Success) continue;

                var startTime = ParseVttTime(timeMatch.Groups[1].Value, timeMatch.Groups[2].Value,
                                           timeMatch.Groups[3].Value, timeMatch.Groups[4].Value);
                var endTime = ParseVttTime(timeMatch.Groups[5].Value, timeMatch.Groups[6].Value,
                                         timeMatch.Groups[7].Value, timeMatch.Groups[8].Value);

                // Collect text lines until next time line or end
                var textLines = new List<string>();
                i++; // Move to next line after time line
                while (i < lines.Length && !VttTimeRegex.IsMatch(lines[i]) && !string.IsNullOrEmpty(lines[i]))
                {
                    if (!string.IsNullOrWhiteSpace(lines[i]))
                        textLines.Add(lines[i]);
                    i++;
                }
                i--; // Adjust for the outer loop increment

                if (textLines.Count > 0)
                {
                    var text = string.Join("\n", textLines);
                    entries.Add(new SubtitleEntry
                    {
                        Index = index++,
                        StartTime = startTime,
                        EndTime = endTime,
                        Text = text
                    });
                }
            }

            return new SubtitleFile
            {
                Entries = new ObservableCollection<SubtitleEntry>(entries),
                Format = SubtitleFormat.VTT
            };
        });
    }

    public Task<string> GenerateAsync(SubtitleFile subtitleFile)
    {
        return Task.Run(() =>
        {
            var vttContent = "WEBVTT\n\n" + string.Join("\n\n", subtitleFile.Entries.Select(entry =>
                $"{FormatVttTime(entry.StartTime)} --> {FormatVttTime(entry.EndTime)}\n{entry.Text}"));

            return vttContent;
        });
    }

    private static TimeSpan ParseVttTime(string hours, string minutes, string seconds, string milliseconds)
    {
        return new TimeSpan(0, int.Parse(hours), int.Parse(minutes), int.Parse(seconds), int.Parse(milliseconds));
    }

    private static string FormatVttTime(TimeSpan time)
    {
        return $"{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2}.{time.Milliseconds:D3}";
    }
} 

