using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using subtitle_sync.ViewModels;
using System;

namespace subtitle_sync;

public partial class App : Application
{
    private IServiceProvider? _serviceProvider;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // 配置依赖注入
        _serviceProvider = Program.ConfigureServices();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // 从DI容器获取MainWindowViewModel
            var mainWindowViewModel = _serviceProvider.GetRequiredService<MainWindowViewModel>();
            
            // 创建主窗口并设置DataContext
            var mainWindow = new MainWindow
            {
                DataContext = mainWindowViewModel
            };
            
            desktop.MainWindow = mainWindow;
            
            // 应用程序退出时释放资源
            desktop.Exit += (_, _) =>
            {
                if (_serviceProvider is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}