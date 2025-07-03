using System.Threading.Tasks;
using subtitle_sync.Models;

namespace subtitle_sync.Services;

/// <summary>
/// 字幕解析器接口
/// </summary>
public interface ISubtitleParser
{
    /// <summary>
    /// 支持的字幕格式
    /// </summary>
    SubtitleFormat SupportedFormat { get; }

    /// <summary>
    /// 检测文件是否为当前格式
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>是否支持</returns>
    Task<bool> CanParseAsync(string filePath);

    /// <summary>
    /// 解析字幕文件
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>字幕文件对象</returns>
    Task<SubtitleFile> ParseAsync(string filePath);

    /// <summary>
    /// 从字符串内容检测是否为当前格式
    /// </summary>
    /// <param name="content">字幕文件内容</param>
    /// <returns>是否支持</returns>
    Task<bool> CanParseFromContentAsync(string content);

    /// <summary>
    /// 从字符串内容解析字幕
    /// </summary>
    /// <param name="content">字幕文件内容</param>
    /// <param name="filename">虚拟文件名（用于标识）</param>
    /// <returns>字幕文件对象</returns>
    Task<SubtitleFile> ParseFromContentAsync(string content, string filename = "test.subtitle");

    /// <summary>
    /// 生成字幕文件内容
    /// </summary>
    /// <param name="subtitleFile">字幕文件对象</param>
    /// <returns>文件内容</returns>
    Task<string> GenerateAsync(SubtitleFile subtitleFile);
} 