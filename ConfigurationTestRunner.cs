using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using subtitle_sync.Services;

namespace subtitle_sync;

/// <summary>
/// 配置功能测试运行器
/// </summary>
public class ConfigurationTestRunner
{
    public async Task RunConfigurationTestsAsync()
    {
        Console.WriteLine("🔧 配置系统测试");
        Console.WriteLine("================");
        
        // 配置依赖注入
        var services = Program.ConfigureServices();
        var configService = services.GetRequiredService<IConfigurationService>();
        var localizationService = services.GetRequiredService<ILocalizationService>();
        
        Console.WriteLine($"📁 配置文件路径: {configService.ConfigurationFilePath}");
        
        // 测试1: 默认配置加载
        Console.WriteLine("\n1. 测试默认配置加载");
        var initialConfig = configService.Load();
        Console.WriteLine($"   初始语言: {initialConfig.Language}");
        Console.WriteLine($"   本地化服务当前语言: {localizationService.CurrentLanguage}");
        
        // 测试2: 语言切换和保存
        Console.WriteLine("\n2. 测试语言切换到中文");
        localizationService.CurrentLanguage = SupportedLanguage.Chinese;
        
        // 等待异步保存完成
        await Task.Delay(500);
        
        // 验证配置文件是否更新
        var updatedConfig = configService.Load();
        Console.WriteLine($"   切换后配置文件语言: {updatedConfig.Language}");
        
        // 测试3: 重新加载验证
        Console.WriteLine("\n3. 测试重新加载配置");
        // 创建新的服务实例模拟程序重启
        var newServices = Program.ConfigureServices();
        var newLocalizationService = newServices.GetRequiredService<ILocalizationService>();
        Console.WriteLine($"   重新加载后语言: {newLocalizationService.CurrentLanguage}");
        
        // 测试4: 切换到日语并验证
        Console.WriteLine("\n4. 测试切换到日语");
        newLocalizationService.CurrentLanguage = SupportedLanguage.Japanese;
        await Task.Delay(500);
        
        var finalConfig = configService.Load();
        Console.WriteLine($"   最终配置文件语言: {finalConfig.Language}");
        
        // 测试5: 查看配置文件内容
        Console.WriteLine("\n5. 当前配置文件内容:");
        if (File.Exists(configService.ConfigurationFilePath))
        {
            var content = await File.ReadAllTextAsync(configService.ConfigurationFilePath);
            Console.WriteLine($"   {content}");
        }
        
        // 恢复到英语
        Console.WriteLine("\n6. 恢复到英语");
        newLocalizationService.CurrentLanguage = SupportedLanguage.English;
        await Task.Delay(500);
        
        Console.WriteLine("✅ 配置系统测试完成！");
        
        // 清理资源
        if (services is IDisposable disposable1) disposable1.Dispose();
        if (newServices is IDisposable disposable2) disposable2.Dispose();
    }
} 