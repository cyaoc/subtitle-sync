using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using subtitle_sync.Models;
using subtitle_sync.Services.Parsers;

namespace subtitle_sync.Services;

/// <summary>
/// 字幕转换服务
/// </summary>
public class SubtitleConverter
{
    private readonly Dictionary<SubtitleFormat, ISubtitleParser> _parsers;
    private readonly ILocalizationService _localization;

    public SubtitleConverter(ILocalizationService localization)
    {
        _localization = localization;
        _parsers = new Dictionary<SubtitleFormat, ISubtitleParser>
        {
            { SubtitleFormat.SRT, new SrtParser() },
            { SubtitleFormat.VTT, new VttParser() },
            { SubtitleFormat.ASS, new AssParser() }
        };
    }

    /// <summary>
    /// 检测文件格式
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>检测到的格式</returns>
    public async Task<SubtitleFormat> DetectFormatAsync(string filePath)
    {
        // 首先通过扩展名判断
        var extension = Path.GetExtension(filePath);
        var formatByExtension = SubtitleFormatExtensions.FromFileExtension(extension);
        
        if (formatByExtension != SubtitleFormat.Unknown && _parsers.ContainsKey(formatByExtension))
        {
            var parser = _parsers[formatByExtension];
            if (await parser.CanParseAsync(filePath))
            {
                return formatByExtension;
            }
        }

        // 如果扩展名检测失败，尝试内容检测
        foreach (var kvp in _parsers)
        {
            if (await kvp.Value.CanParseAsync(filePath))
            {
                return kvp.Key;
            }
        }

        return SubtitleFormat.Unknown;
    }

    /// <summary>
    /// 从字符串内容检测格式
    /// </summary>
    /// <param name="content">字幕文件内容</param>
    /// <returns>检测到的格式</returns>
    public async Task<SubtitleFormat> DetectFormatFromContentAsync(string content)
    {
        // 尝试各种解析器进行内容检测
        foreach (var kvp in _parsers)
        {
            if (await kvp.Value.CanParseFromContentAsync(content))
            {
                return kvp.Key;
            }
        }

        return SubtitleFormat.Unknown;
    }

    /// <summary>
    /// 解析字幕文件
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>字幕文件对象</returns>
    public async Task<SubtitleFile?> ParseAsync(string filePath)
    {
        var format = await DetectFormatAsync(filePath);
        if (format == SubtitleFormat.Unknown || !_parsers.ContainsKey(format))
        {
            return null;
        }

        var parser = _parsers[format];
        return await parser.ParseAsync(filePath);
    }

    /// <summary>
    /// 从字符串内容解析字幕
    /// </summary>
    /// <param name="content">字幕文件内容</param>
    /// <param name="filename">虚拟文件名（用于标识）</param>
    /// <returns>字幕文件对象</returns>
    public async Task<SubtitleFile?> ParseFromContentAsync(string content, string filename = "test.subtitle")
    {
        var format = await DetectFormatFromContentAsync(content);
        if (format == SubtitleFormat.Unknown || !_parsers.ContainsKey(format))
        {
            return null;
        }

        var parser = _parsers[format];
        return await parser.ParseFromContentAsync(content, filename);
    }

    /// <summary>
    /// 转换字幕格式
    /// </summary>
    /// <param name="sourceFile">源字幕文件</param>
    /// <param name="targetFormat">目标格式</param>
    /// <returns>转换后的文件内容</returns>
    public async Task<string?> ConvertAsync(SubtitleFile sourceFile, SubtitleFormat targetFormat)
    {
        if (!_parsers.ContainsKey(targetFormat))
        {
            return null;
        }

        var targetParser = _parsers[targetFormat];
        
        // 创建新的字幕文件对象
        var targetFile = new SubtitleFile
        {
            Format = targetFormat,
            Encoding = sourceFile.Encoding,
            Title = sourceFile.Title
        };

        // 复制字幕条目
        foreach (var entry in sourceFile.Entries)
        {
            targetFile.AddEntry(new SubtitleEntry(entry.Index, entry.StartTime, entry.EndTime, entry.Text, entry.Style));
        }

        return await targetParser.GenerateAsync(targetFile);
    }

    /// <summary>
    /// 从字符串内容转换字幕格式
    /// </summary>
    /// <param name="sourceContent">源字幕内容</param>
    /// <param name="targetFormat">目标格式</param>
    /// <param name="sourceFilename">源文件名（用于标识）</param>
    /// <returns>转换后的文件内容</returns>
    public async Task<string?> ConvertFromContentAsync(string sourceContent, SubtitleFormat targetFormat, string sourceFilename = "test.subtitle")
    {
        var sourceFile = await ParseFromContentAsync(sourceContent, sourceFilename);
        if (sourceFile == null || !sourceFile.IsValid())
        {
            return null;
        }

        return await ConvertAsync(sourceFile, targetFormat);
    }

    /// <summary>
    /// 转换并保存字幕文件
    /// </summary>
    /// <param name="sourceFilePath">源文件路径</param>
    /// <param name="targetFilePath">目标文件路径</param>
    /// <param name="targetFormat">目标格式</param>
    /// <returns>是否成功</returns>
    public async Task<bool> ConvertAndSaveAsync(string sourceFilePath, string targetFilePath, SubtitleFormat targetFormat)
    {
        try
        {
            var sourceFile = await ParseAsync(sourceFilePath);
            if (sourceFile == null || !sourceFile.IsValid())
            {
                return false;
            }

            var content = await ConvertAsync(sourceFile, targetFormat);
            if (content == null)
            {
                return false;
            }

            await File.WriteAllTextAsync(targetFilePath, content, System.Text.Encoding.UTF8);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 获取支持的格式列表
    /// </summary>
    /// <returns>支持的格式列表</returns>
    public IEnumerable<SubtitleFormat> GetSupportedFormats()
    {
        return _parsers.Keys;
    }

    /// <summary>
    /// 验证字幕文件
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>验证结果</returns>
    public async Task<(bool IsValid, string Message, SubtitleFormat Format)> ValidateAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return (false, _localization["FileNotFound"], SubtitleFormat.Unknown);
            }

            var format = await DetectFormatAsync(filePath);
            if (format == SubtitleFormat.Unknown)
            {
                return (false, _localization["UnrecognizedFormat"], SubtitleFormat.Unknown);
            }

            var subtitleFile = await ParseAsync(filePath);
            if (subtitleFile == null)
            {
                return (false, _localization["ParseFailed"], format);
            }

            if (!subtitleFile.IsValid())
            {
                return (false, _localization["InvalidFormat"], format);
            }

            return (true, string.Format(_localization["ValidFileMessage"], format.GetDisplayName(), subtitleFile.Count), format);
        }
        catch (Exception ex)
        {
            return (false, string.Format(_localization["ValidationFailed"], ex.Message), SubtitleFormat.Unknown);
        }
    }

    /// <summary>
    /// 验证字符串内容中的字幕
    /// </summary>
    /// <param name="content">字幕文件内容</param>
    /// <param name="filename">虚拟文件名</param>
    /// <returns>验证结果</returns>
    public async Task<(bool IsValid, string Message, SubtitleFormat Format)> ValidateFromContentAsync(string content, string filename = "test.subtitle")
    {
        try
        {
            var format = await DetectFormatFromContentAsync(content);
            if (format == SubtitleFormat.Unknown)
            {
                return (false, _localization["UnrecognizedFormat"], SubtitleFormat.Unknown);
            }

            var subtitleFile = await ParseFromContentAsync(content, filename);
            if (subtitleFile == null)
            {
                return (false, _localization["ParseFailed"], format);
            }

            if (!subtitleFile.IsValid())
            {
                return (false, _localization["InvalidFormat"], format);
            }

            return (true, string.Format(_localization["ValidFileMessage"], format.GetDisplayName(), subtitleFile.Count), format);
        }
        catch (Exception ex)
        {
            return (false, string.Format(_localization["ValidationFailed"], ex.Message), SubtitleFormat.Unknown);
        }
    }
} 