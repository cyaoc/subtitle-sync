#!/bin/bash

# 字幕转换工具测试脚本
echo "=== 字幕转换工具核心功能测试 ==="
echo "=================================="
echo

# 检查是否已编译
if [ ! -f "bin/Debug/net9.0/subtitle-sync.dll" ]; then
    echo "📦 项目尚未编译，正在编译..."
    dotnet build
    if [ $? -ne 0 ]; then
        echo "❌ 编译失败，请检查错误信息"
        exit 1
    fi
    echo "✅ 编译完成"
    echo
fi

# 运行测试
echo "🚀 开始运行测试..."
echo

dotnet run -- --test

echo
echo "测试完成！" 