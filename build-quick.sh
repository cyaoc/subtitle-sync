#!/bin/bash

# 快速构建脚本 - 仅构建当前平台和Windows
set -e

echo "🚀 快速构建字幕转换工具"
echo "========================="

# 创建输出目录
mkdir -p releases

# 检测当前平台
if [[ "$OSTYPE" == "darwin"* ]]; then
    if [[ $(uname -m) == "arm64" ]]; then
        CURRENT_RUNTIME="osx-arm64"
        PLATFORM_NAME="macOS (Apple Silicon)"
    else
        CURRENT_RUNTIME="osx-x64" 
        PLATFORM_NAME="macOS (Intel)"
    fi
elif [[ "$OSTYPE" == "linux-gnu"* ]]; then
    CURRENT_RUNTIME="linux-x64"
    PLATFORM_NAME="Linux"
else
    CURRENT_RUNTIME="win-x64"
    PLATFORM_NAME="Windows"
fi

echo "检测到平台: $PLATFORM_NAME"

# 构建当前平台 (Standalone)
echo ""
echo "🔨 构建 $PLATFORM_NAME (包含运行时)..."
dotnet publish subtitle-sync.csproj \
    --configuration Release \
    --runtime $CURRENT_RUNTIME \
    --self-contained true \
    --output "releases/current-standalone" \
    -p:PublishSingleFile=true \
    -p:PublishTrimmed=true \
    -p:DebugType=None \
    -p:DebugSymbols=false

if [ $? -eq 0 ]; then
    echo "✅ 当前平台构建成功!"
else
    echo "❌ 当前平台构建失败!"
    exit 1
fi

# 构建当前平台 (Framework-dependent)
echo ""
echo "🔨 构建 $PLATFORM_NAME (框架依赖)..."
dotnet publish subtitle-sync.csproj \
    --configuration Release \
    --runtime $CURRENT_RUNTIME \
    --self-contained false \
    --output "releases/current-framework" \
    -p:PublishSingleFile=true \
    -p:DebugType=None \
    -p:DebugSymbols=false

if [ $? -eq 0 ]; then
    echo "✅ 框架依赖版本构建成功!"
else
    echo "❌ 框架依赖版本构建失败!"
fi

# 构建 Windows (Standalone) - 如果不是Windows平台
if [[ "$CURRENT_RUNTIME" != "win-x64" ]]; then
    echo ""
    echo "🔨 构建 Windows (包含运行时)..."
    dotnet publish subtitle-sync.csproj \
        --configuration Release \
        --runtime win-x64 \
        --self-contained true \
        --output "releases/windows-standalone" \
        -p:PublishSingleFile=true \
        -p:PublishTrimmed=true \
        -p:DebugType=None \
        -p:DebugSymbols=false

    if [ $? -eq 0 ]; then
        echo "✅ Windows 版本构建成功!"
    else
        echo "❌ Windows 版本构建失败!"
    fi
fi

# 显示构建结果
echo ""
echo "📦 构建完成! 生成的文件:"
echo "========================"

find releases -name "subtitle-sync*" -type f | while read file; do
    size=$(stat -f%z "$file" 2>/dev/null || stat -c%s "$file" 2>/dev/null || echo "0")
    size_mb=$(echo "scale=1; $size / 1024 / 1024" | bc 2>/dev/null || echo "0")
    echo "  $(basename "$(dirname "$file")")/$(basename "$file") ($size_mb MB)"
done

echo ""
echo "🎉 构建成功! 可以在 releases/ 目录中找到可执行文件。"

# 设置当前平台可执行文件权限
chmod +x releases/*/subtitle-sync* 2>/dev/null || true 