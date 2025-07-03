using System.Collections.Generic;
using System.ComponentModel;

namespace subtitle_sync.Services;

public enum SupportedLanguage
{
    English,
    Chinese,
    Japanese
}

/// <summary>
/// 本地化服务接口
/// </summary>
public interface ILocalizationService : INotifyPropertyChanged
{
    /// <summary>
    /// 当前语言
    /// </summary>
    SupportedLanguage CurrentLanguage { get; set; }

    /// <summary>
    /// 获取本地化字符串
    /// </summary>
    /// <param name="key">键</param>
    /// <returns>本地化字符串</returns>
    string this[string key] { get; }

    /// <summary>
    /// 获取支持的语言列表
    /// </summary>
    /// <returns>语言项目列表</returns>
    List<LanguageItem> GetSupportedLanguages();
}

/// <summary>
/// 语言项目
/// </summary>
public class LanguageItem
{
    public SupportedLanguage Language { get; }
    public string DisplayName { get; }

    public LanguageItem(SupportedLanguage language, string displayName)
    {
        Language = language;
        DisplayName = displayName;
    }

    public override string ToString() => DisplayName;
} 