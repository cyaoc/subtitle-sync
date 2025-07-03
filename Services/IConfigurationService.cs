using System;
using System.Threading.Tasks;
using subtitle_sync.Models.Configuration;

namespace subtitle_sync.Services;

/// <summary>
/// 配置服务接口
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// 加载配置
    /// </summary>
    /// <returns>应用程序配置</returns>
    AppConfiguration Load();

    /// <summary>
    /// 异步保存配置
    /// </summary>
    /// <param name="configuration">要保存的配置</param>
    Task SaveAsync(AppConfiguration configuration);

    /// <summary>
    /// 同步保存配置
    /// </summary>
    /// <param name="configuration">要保存的配置</param>
    void Save(AppConfiguration configuration);

    /// <summary>
    /// 获取配置文件路径
    /// </summary>
    string ConfigurationFilePath { get; }

    /// <summary>
    /// 配置更改事件
    /// </summary>
    event EventHandler<AppConfiguration>? ConfigurationChanged;
} 