using Avalonia;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using subtitle_sync.Services;
using subtitle_sync.ViewModels;

namespace subtitle_sync;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        // 检查是否运行测试模式
        if (args.Length > 0 && args[0] == "--test")
        {
            RunTestsAsync().GetAwaiter().GetResult();
            return;
        }

        // 检查是否运行配置测试模式
        if (args.Length > 0 && args[0] == "--test-config")
        {
            RunConfigurationTestsAsync().GetAwaiter().GetResult();
            return;
        }

        // 正常启动GUI应用
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    /// <summary>
    /// 运行测试
    /// </summary>
    private static async Task RunTestsAsync()
    {
        Console.WriteLine("启动字幕转换测试程序...");
        Console.WriteLine();

        try
        {
            var testRunner = new SubtitleTestRunner();
            await testRunner.RunAllTestsAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"测试运行出错: {ex.Message}");
            Console.WriteLine($"详细错误: {ex.StackTrace}");
        }

        Console.WriteLine();
        Console.WriteLine("按任意键退出...");
        Console.ReadKey();
    }

    /// <summary>
    /// 运行配置系统测试
    /// </summary>
    private static async Task RunConfigurationTestsAsync()
    {
        Console.WriteLine("启动配置系统测试程序...");
        Console.WriteLine();

        try
        {
            var configTestRunner = new ConfigurationTestRunner();
            await configTestRunner.RunConfigurationTestsAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"配置测试运行出错: {ex.Message}");
            Console.WriteLine($"详细错误: {ex.StackTrace}");
        }

        Console.WriteLine();
        Console.WriteLine("按任意键退出...");
        Console.ReadKey();
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();

    /// <summary>
    /// 配置依赖注入服务
    /// </summary>
    public static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // 注册配置服务
        services.AddSingleton<IConfigurationService, ConfigurationService>();

        // 注册本地化服务
        services.AddSingleton<ILocalizationService, LocalizationService>();

        // 注册解析器服务
        services.AddSingleton<ISubtitleParser, subtitle_sync.Services.Parsers.SrtParser>();
        services.AddSingleton<ISubtitleParser, subtitle_sync.Services.Parsers.VttParser>();
        services.AddSingleton<ISubtitleParser, subtitle_sync.Services.Parsers.AssParser>();

        // 注册转换服务
        services.AddSingleton<SubtitleConverter>();

        // 注册ViewModels
        services.AddTransient<MainWindowViewModel>();

        return services.BuildServiceProvider();
    }
}
