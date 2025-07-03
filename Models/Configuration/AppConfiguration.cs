using YamlDotNet.Serialization;

namespace subtitle_sync.Models.Configuration;

/// <summary>
/// 应用程序配置 - 简化版本，只包含必要的语言设置
/// </summary>
public class AppConfiguration
{
    /// <summary>
    /// 界面语言 (English, Chinese, Japanese)
    /// </summary>
    [YamlMember(Alias = "language")]
    public string Language { get; set; } = "English";
} 