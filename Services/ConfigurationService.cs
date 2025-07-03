using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using subtitle_sync.Models.Configuration;

namespace subtitle_sync.Services;

/// <summary>
/// 配置服务实现
/// </summary>
public class ConfigurationService : IConfigurationService
{
    private readonly string _configurationDirectory;
    private readonly string _configurationFilePath;
    private readonly ISerializer _yamlSerializer;
    private readonly IDeserializer _yamlDeserializer;
    private AppConfiguration? _cachedConfiguration;

    public string ConfigurationFilePath => _configurationFilePath;

    public event EventHandler<AppConfiguration>? ConfigurationChanged;

    public ConfigurationService()
    {
        _configurationDirectory = GetConfigurationDirectory();
        _configurationFilePath = Path.Combine(_configurationDirectory, "config.yaml");

        // 初始化YAML序列化器
        _yamlSerializer = new SerializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();

        _yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    /// <summary>
    /// 加载配置
    /// </summary>
    public AppConfiguration Load()
    {
        try
        {
            if (_cachedConfiguration != null)
                return _cachedConfiguration;

            if (!File.Exists(_configurationFilePath))
            {
                // 配置文件不存在，创建默认配置
                var defaultConfig = CreateDefaultConfiguration();
                Save(defaultConfig);
                _cachedConfiguration = defaultConfig;
                return defaultConfig;
            }

            var yamlContent = File.ReadAllText(_configurationFilePath);
            var configuration = _yamlDeserializer.Deserialize<AppConfiguration>(yamlContent);
            
            // 验证并修复配置
            configuration = ValidateConfiguration(configuration);
            _cachedConfiguration = configuration;
            
            return configuration;
        }
        catch (Exception)
        {
            // 配置文件损坏或读取失败，返回默认配置
            var defaultConfig = CreateDefaultConfiguration();
            _cachedConfiguration = defaultConfig;
            return defaultConfig;
        }
    }

    /// <summary>
    /// 异步保存配置
    /// </summary>
    public async Task SaveAsync(AppConfiguration configuration)
    {
        await Task.Run(() => Save(configuration));
    }

    /// <summary>
    /// 同步保存配置
    /// </summary>
    public void Save(AppConfiguration configuration)
    {
        try
        {
            // 确保配置目录存在
            Directory.CreateDirectory(_configurationDirectory);

            // 序列化配置为YAML
            var yamlContent = _yamlSerializer.Serialize(configuration);
            
            // 添加文件头注释
            var fileContent = GenerateFileHeader() + yamlContent;
            
            // 写入文件
            File.WriteAllText(_configurationFilePath, fileContent);
            
            // 更新缓存
            _cachedConfiguration = configuration;
            
            // 触发配置更改事件
            ConfigurationChanged?.Invoke(this, configuration);
        }
        catch (Exception)
        {
            // 保存失败时静默处理，避免影响应用程序运行
        }
    }

    /// <summary>
    /// 创建默认配置
    /// </summary>
    private static AppConfiguration CreateDefaultConfiguration()
    {
        var config = new AppConfiguration();
        
        // 尝试检测系统语言
        var systemLanguage = DetectSystemLanguage();
        config.Language = systemLanguage;
        
        return config;
    }

    /// <summary>
    /// 检测系统语言
    /// </summary>
    private static string DetectSystemLanguage()
    {
        try
        {
            var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            return culture switch
            {
                "zh" => "Chinese",
                "ja" => "Japanese",
                _ => "English"
            };
        }
        catch
        {
            return "English";
        }
    }

    /// <summary>
    /// 验证和修复配置
    /// </summary>
    private static AppConfiguration ValidateConfiguration(AppConfiguration configuration)
    {
        configuration ??= new AppConfiguration();
        
        // 验证语言设置
        var validLanguages = new[] { "English", "Chinese", "Japanese" };
        if (!validLanguages.Contains(configuration.Language))
        {
            configuration.Language = DetectSystemLanguage();
        }

        return configuration;
    }

    /// <summary>
    /// 获取配置目录 - 使用程序所在目录
    /// </summary>
    private static string GetConfigurationDirectory()
    {
        // 对于单文件发布，使用AppContext.BaseDirectory
        return AppContext.BaseDirectory;
    }

    /// <summary>
    /// 生成配置文件头部注释
    /// </summary>
    private static string GenerateFileHeader()
    {
        return $"""
# SubtitleSync Language Configuration
# Language options: English, Chinese, Japanese
#

""";
    }
} 