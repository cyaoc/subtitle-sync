# 字幕转换工具构建脚本 (Windows PowerShell)
param(
    [switch]$Clean = $false,
    [string]$Configuration = "Release"
)

Write-Host "字幕转换工具构建脚本" -ForegroundColor Green
Write-Host "============================" -ForegroundColor Green

# 项目信息
$ProjectName = "subtitle-sync"
$ProjectFile = "subtitle-sync.csproj"
$OutputDir = "releases"
$Version = "1.0.0"

# 清理输出目录
if ($Clean -or (Test-Path $OutputDir)) {
    Write-Host "清理输出目录..." -ForegroundColor Yellow
    Remove-Item $OutputDir -Recurse -Force -ErrorAction SilentlyContinue
}

# 创建输出目录
New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null

# 定义构建目标
$BuildTargets = @(
    @{
        Runtime = "win-x64"
        Platform = "Windows"
        SelfContained = $true
        Description = "Windows (x64) - 包含运行时"
        OutputName = "$ProjectName-$Version-win-x64-standalone"
    },
    @{
        Runtime = "win-x64"
        Platform = "Windows"
        SelfContained = $false
        Description = "Windows (x64) - 需要 .NET 9"
        OutputName = "$ProjectName-$Version-win-x64-framework"
    },
    @{
        Runtime = "osx-x64"
        Platform = "macOS"
        SelfContained = $true
        Description = "macOS (x64) - 包含运行时"
        OutputName = "$ProjectName-$Version-osx-x64-standalone"
    },
    @{
        Runtime = "osx-x64"
        Platform = "macOS"
        SelfContained = $false
        Description = "macOS (x64) - 需要 .NET 9"
        OutputName = "$ProjectName-$Version-osx-x64-framework"
    },
    @{
        Runtime = "osx-arm64"
        Platform = "macOS"
        SelfContained = $true
        Description = "macOS (ARM64) - 包含运行时"
        OutputName = "$ProjectName-$Version-osx-arm64-standalone"
    },
    @{
        Runtime = "osx-arm64"
        Platform = "macOS"
        SelfContained = $false
        Description = "macOS (ARM64) - 需要 .NET 9"
        OutputName = "$ProjectName-$Version-osx-arm64-framework"
    }
)

# 构建函数
function Build-Target {
    param($Target)
    
    Write-Host "`n正在构建: $($Target.Description)" -ForegroundColor Cyan
    Write-Host "运行时: $($Target.Runtime)" -ForegroundColor Gray
    Write-Host "独立部署: $($Target.SelfContained)" -ForegroundColor Gray
    
    $OutputPath = "$OutputDir\$($Target.OutputName)"
    
    # 构建参数
    $BuildArgs = @(
        "publish"
        $ProjectFile
        "--configuration", $Configuration
        "--runtime", $Target.Runtime
        "--self-contained", $Target.SelfContained.ToString().ToLower()
        "--output", $OutputPath
        "-p:PublishSingleFile=true"
        "-p:PublishTrimmed=true"
        "-p:DebugType=None"
        "-p:DebugSymbols=false"
    )
    
    # 执行构建
    & dotnet @BuildArgs
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ 构建成功: $($Target.OutputName)" -ForegroundColor Green
        
        # 创建压缩包
        $ZipPath = "$OutputDir\$($Target.OutputName).zip"
        Compress-Archive -Path "$OutputPath\*" -DestinationPath $ZipPath -Force
        Write-Host "📦 已创建压缩包: $($Target.OutputName).zip" -ForegroundColor Green
    }
    else {
        Write-Host "❌ 构建失败: $($Target.OutputName)" -ForegroundColor Red
    }
}

# 检查 .NET SDK
Write-Host "`n检查 .NET SDK..." -ForegroundColor Yellow
$DotNetVersion = & dotnet --version 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ 未找到 .NET SDK，请先安装 .NET 9" -ForegroundColor Red
    exit 1
}
Write-Host "✅ .NET SDK 版本: $DotNetVersion" -ForegroundColor Green

# 执行构建
Write-Host "`n开始构建所有目标..." -ForegroundColor Yellow
$SuccessCount = 0
$TotalCount = $BuildTargets.Count

foreach ($Target in $BuildTargets) {
    try {
        Build-Target -Target $Target
        $SuccessCount++
    }
    catch {
        Write-Host "❌ 构建失败: $($Target.OutputName) - $($_.Exception.Message)" -ForegroundColor Red
    }
}

# 构建摘要
Write-Host "`n构建摘要" -ForegroundColor Green
Write-Host "=========" -ForegroundColor Green
Write-Host "成功: $SuccessCount/$TotalCount" -ForegroundColor $(if ($SuccessCount -eq $TotalCount) { "Green" } else { "Yellow" })
Write-Host "输出目录: $((Resolve-Path $OutputDir).Path)" -ForegroundColor Gray

# 列出生成的文件
Write-Host "`n生成的文件:" -ForegroundColor Yellow
Get-ChildItem $OutputDir -Recurse -File | ForEach-Object {
    $Size = [math]::Round($_.Length / 1MB, 2)
    Write-Host "  $($_.Name) ($Size MB)" -ForegroundColor Gray
}

Write-Host "`n构建完成！" -ForegroundColor Green 