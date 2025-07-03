# 字幕转换工具 - 构建指南

## 🚀 快速构建

### Windows (PowerShell)
```powershell
# 构建所有平台版本
.\build.ps1

# 清理重新构建
.\build.ps1 -Clean

# 构建调试版本
.\build.ps1 -Configuration Debug
```

### macOS/Linux (Bash)
```bash
# 构建所有平台版本
./build.sh

# 清理重新构建
./build.sh --clean

# 构建调试版本
./build.sh --configuration Debug
```

## 📦 输出文件

构建完成后，在 `releases/` 目录下会生成以下文件：

### Windows 平台
- `subtitle-sync-1.0.0-win-x64-standalone.zip` - **包含 .NET 9 运行时**（推荐）
- `subtitle-sync-1.0.0-win-x64-framework.zip` - 需要系统安装 .NET 9

### macOS 平台
- `subtitle-sync-1.0.0-osx-x64-standalone.tar.gz` - Intel Mac，包含运行时
- `subtitle-sync-1.0.0-osx-x64-framework.tar.gz` - Intel Mac，需要 .NET 9
- `subtitle-sync-1.0.0-osx-arm64-standalone.tar.gz` - Apple Silicon Mac，包含运行时
- `subtitle-sync-1.0.0-osx-arm64-framework.tar.gz` - Apple Silicon Mac，需要 .NET 9

### Linux 平台 (仅 bash 脚本生成)
- `subtitle-sync-1.0.0-linux-x64-standalone.tar.gz` - 包含运行时
- `subtitle-sync-1.0.0-linux-x64-framework.tar.gz` - 需要 .NET 9

## 🎯 版本类型说明

### Standalone (独立部署)
- ✅ **优点**：无需用户安装 .NET 9，开箱即用
- ❌ **缺点**：文件较大（约 50-80MB）
- 🎯 **推荐**：给最终用户使用

### Framework Dependent (框架依赖)
- ✅ **优点**：文件很小（约 1-5MB）
- ❌ **缺点**：需要用户事先安装 .NET 9 运行时
- 🎯 **推荐**：开发者或已安装 .NET 的用户

## 🛠️ 手动构建命令

如果你想手动构建特定平台：

### Windows (包含运行时)
```bash
dotnet publish --configuration Release --runtime win-x64 --self-contained true --output releases/win-standalone -p:PublishSingleFile=true -p:PublishTrimmed=true
```

### macOS Intel (包含运行时)
```bash
dotnet publish --configuration Release --runtime osx-x64 --self-contained true --output releases/mac-intel-standalone -p:PublishSingleFile=true -p:PublishTrimmed=true
```

### macOS Apple Silicon (包含运行时)
```bash
dotnet publish --configuration Release --runtime osx-arm64 --self-contained true --output releases/mac-arm-standalone -p:PublishSingleFile=true -p:PublishTrimmed=true
```

## 📋 系统要求

### 构建环境
- .NET 9 SDK
- Windows: PowerShell 5.0+
- macOS/Linux: Bash 4.0+

### 运行环境
- **Standalone 版本**：无额外要求
- **Framework 版本**：需要安装 .NET 9 运行时

## 🔧 构建参数

### PowerShell 脚本参数
- `-Clean`: 清理输出目录后构建
- `-Configuration`: 构建配置 (Release/Debug)

### Bash 脚本参数
- `--clean`: 清理输出目录后构建
- `--configuration`: 构建配置 (Release/Debug)

## 📝 发布说明

### v1.0.0 特性
- 🌍 多语言界面 (英文/中文/日文)
- 📁 拖拽文件支持
- 🔄 多格式转换 (SRT/VTT/ASS)
- 📊 字幕信息显示
- ⚡ 快速格式检测
- 🎨 现代化 UI 设计

## 🐛 故障排除

### 构建失败常见问题

1. **找不到 .NET SDK**
   ```bash
   # 安装 .NET 9 SDK
   # 下载地址: https://dotnet.microsoft.com/download/dotnet/9.0
   ```

2. **权限错误** (macOS/Linux)
   ```bash
   chmod +x build.sh
   ```

3. **PowerShell 执行策略** (Windows)
   ```powershell
   Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
   ```

### 运行时问题

1. **Framework 版本无法启动**
   - 安装 .NET 9 运行时：https://dotnet.microsoft.com/download/dotnet/9.0

2. **macOS 安全提示**
   ```bash
   # 取消隔离
   xattr -d com.apple.quarantine subtitle-sync
   ```

## 📞 支持

如有问题，请查看：
1. 确保 .NET 9 SDK 已正确安装
2. 检查系统架构是否匹配 (x64/ARM64)
3. 查看构建日志中的错误信息 