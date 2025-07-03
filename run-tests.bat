@echo off
chcp 65001 >nul
echo === 字幕转换工具核心功能测试 ===
echo ==================================
echo.

REM 检查是否已编译
if not exist "bin\Debug\net9.0\subtitle-sync.dll" (
    echo 📦 项目尚未编译，正在编译...
    dotnet build
    if errorlevel 1 (
        echo ❌ 编译失败，请检查错误信息
        pause
        exit /b 1
    )
    echo ✅ 编译完成
    echo.
)

REM 运行测试
echo 🚀 开始运行测试...
echo.

dotnet run -- --test

echo.
echo 测试完成！
pause 