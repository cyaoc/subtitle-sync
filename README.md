# 字幕转换工具

一个简单的字幕格式转换工具，支持 SRT、VTT、ASS 格式之间的相互转换。

![License](https://img.shields.io/badge/License-MIT-green)
![.NET](https://img.shields.io/badge/.NET-9.0-purple)
![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20macOS%20%7C%20Linux-blue)

## 关于这个项目

这是我使用 [Cursor](https://cursor.sh/) 和 AI 编程的一次尝试。整个项目主要通过与 AI 对话来完成开发，算是探索 AI 辅助编程的一个小实验。

技术栈：
- .NET 9 + Avalonia UI
- 桌面跨平台应用

## 功能

- 支持 SRT、VTT、ASS 字幕格式转换
- 拖拽文件操作
- 多语言界面（中文/英文/日文）
- 跨平台（Windows/macOS/Linux）

## 使用方法

1. 下载对应平台的版本到 [releases](https://github.com/cyaoc/subtitle-sync/releases) 目录
2. 运行程序
3. 拖拽字幕文件或点击选择文件
4. 选择目标格式
5. 点击转换

## 构建

需要 .NET 9 SDK：

```bash
# macOS/Linux
./build.sh

# Windows
./build.ps1

# 快速构建（当前平台）
./build-quick.sh
```

## 测试

```bash
# macOS/Linux
./run-tests.sh

# Windows
run-tests.bat

# 或者直接运行
dotnet run -- --test
```

## 许可证

MIT License

## 致谢

感谢以下开源项目：

- [Avalonia UI](https://avaloniaui.net/) - 跨平台 UI 框架
- [.NET](https://dotnet.microsoft.com/) - 应用程序框架
- [YamlDotNet](https://github.com/aaubry/YamlDotNet) - YAML 配置支持
- [Cursor](https://cursor.sh/) - AI 代码编辑器，让这个项目成为可能

以及所有其他间接使用的开源库和工具。 
