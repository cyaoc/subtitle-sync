#!/bin/bash

# 字幕转换工具构建脚本 (macOS/Linux)

set -e  # 遇到错误立即退出

# 颜色定义
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
GRAY='\033[0;37m'
NC='\033[0m' # No Color

# 项目信息
PROJECT_NAME="subtitle-sync"
PROJECT_FILE="subtitle-sync.csproj"
OUTPUT_DIR="releases"
VERSION="1.0.0"
CONFIGURATION="Release"

# 解析命令行参数
CLEAN=false
while [[ $# -gt 0 ]]; do
    case $1 in
        --clean)
            CLEAN=true
            shift
            ;;
        --configuration)
            CONFIGURATION="$2"
            shift 2
            ;;
        *)
            echo -e "${RED}未知参数: $1${NC}"
            echo "用法: $0 [--clean] [--configuration Release|Debug]"
            exit 1
            ;;
    esac
done

echo -e "${GREEN}字幕转换工具构建脚本${NC}"
echo -e "${GREEN}============================${NC}"

# 清理输出目录
if [ "$CLEAN" = true ] || [ -d "$OUTPUT_DIR" ]; then
    echo -e "${YELLOW}清理输出目录...${NC}"
    rm -rf "$OUTPUT_DIR"
fi

# 创建输出目录
mkdir -p "$OUTPUT_DIR"

# 定义构建目标
declare -a BUILD_TARGETS=(
    "win-x64:Windows:true:Windows (x64) - 包含运行时:$PROJECT_NAME-$VERSION-win-x64-standalone"
    "win-x64:Windows:false:Windows (x64) - 需要 .NET 9:$PROJECT_NAME-$VERSION-win-x64-framework"
    "osx-x64:macOS:true:macOS (x64) - 包含运行时:$PROJECT_NAME-$VERSION-osx-x64-standalone"
    "osx-x64:macOS:false:macOS (x64) - 需要 .NET 9:$PROJECT_NAME-$VERSION-osx-x64-framework"
    "osx-arm64:macOS:true:macOS (ARM64) - 包含运行时:$PROJECT_NAME-$VERSION-osx-arm64-standalone"
    "osx-arm64:macOS:false:macOS (ARM64) - 需要 .NET 9:$PROJECT_NAME-$VERSION-osx-arm64-framework"
    "linux-x64:Linux:true:Linux (x64) - 包含运行时:$PROJECT_NAME-$VERSION-linux-x64-standalone"
    "linux-x64:Linux:false:Linux (x64) - 需要 .NET 9:$PROJECT_NAME-$VERSION-linux-x64-framework"
)

# 构建函数
build_target() {
    local runtime="$1"
    local platform="$2"
    local self_contained="$3"
    local description="$4"
    local output_name="$5"
    
    echo ""
    echo -e "${CYAN}正在构建: $description${NC}"
    echo -e "${GRAY}运行时: $runtime${NC}"
    echo -e "${GRAY}独立部署: $self_contained${NC}"
    
    local output_path="$OUTPUT_DIR/$output_name"
    
    # 构建参数
    local build_args=(
        "publish"
        "$PROJECT_FILE"
        "--configuration" "$CONFIGURATION"
        "--runtime" "$runtime"
        "--self-contained" "$self_contained"
        "--output" "$output_path"
        "-p:PublishSingleFile=true"
        "-p:DebugType=None"
        "-p:DebugSymbols=false"
    )
    
    # 只在独立部署时启用代码裁剪
    if [ "$self_contained" = "true" ]; then
        build_args+=("-p:PublishTrimmed=true")
    fi
    
    # 执行构建
    if dotnet "${build_args[@]}"; then
        echo -e "${GREEN}✅ 构建成功: $output_name${NC}"
        
        # 创建压缩包
        local zip_path="$OUTPUT_DIR/$output_name.tar.gz"
        if command -v tar >/dev/null 2>&1; then
            cd "$output_path"
            tar -czf "../$output_name.tar.gz" *
            cd - >/dev/null
            echo -e "${GREEN}📦 已创建压缩包: $output_name.tar.gz${NC}"
        fi
        
        return 0
    else
        echo -e "${RED}❌ 构建失败: $output_name${NC}"
        return 1
    fi
}

# 检查 .NET SDK
echo ""
echo -e "${YELLOW}检查 .NET SDK...${NC}"
if ! command -v dotnet >/dev/null 2>&1; then
    echo -e "${RED}❌ 未找到 .NET SDK，请先安装 .NET 9${NC}"
    exit 1
fi

DOTNET_VERSION=$(dotnet --version)
echo -e "${GREEN}✅ .NET SDK 版本: $DOTNET_VERSION${NC}"

# 执行构建
echo ""
echo -e "${YELLOW}开始构建所有目标...${NC}"
SUCCESS_COUNT=0
TOTAL_COUNT=${#BUILD_TARGETS[@]}

for target in "${BUILD_TARGETS[@]}"; do
    IFS=':' read -ra TARGET_PARTS <<< "$target"
    runtime="${TARGET_PARTS[0]}"
    platform="${TARGET_PARTS[1]}"
    self_contained="${TARGET_PARTS[2]}"
    description="${TARGET_PARTS[3]}"
    output_name="${TARGET_PARTS[4]}"
    
    if build_target "$runtime" "$platform" "$self_contained" "$description" "$output_name"; then
        ((SUCCESS_COUNT++))
    fi
done

# 构建摘要
echo ""
echo -e "${GREEN}构建摘要${NC}"
echo -e "${GREEN}=========${NC}"

if [ $SUCCESS_COUNT -eq $TOTAL_COUNT ]; then
    echo -e "${GREEN}成功: $SUCCESS_COUNT/$TOTAL_COUNT${NC}"
else
    echo -e "${YELLOW}成功: $SUCCESS_COUNT/$TOTAL_COUNT${NC}"
fi

echo -e "${GRAY}输出目录: $(pwd)/$OUTPUT_DIR${NC}"

# 列出生成的文件
echo ""
echo -e "${YELLOW}生成的文件:${NC}"
if command -v find >/dev/null 2>&1; then
    find "$OUTPUT_DIR" -type f \( -name "*.tar.gz" -o -name "$PROJECT_NAME*" \) | while read -r file; do
        if command -v stat >/dev/null 2>&1; then
            if [[ "$OSTYPE" == "darwin"* ]]; then
                # macOS
                size=$(stat -f%z "$file" 2>/dev/null || echo "0")
            else
                # Linux
                size=$(stat -c%s "$file" 2>/dev/null || echo "0")
            fi
            size_mb=$(echo "scale=2; $size / 1024 / 1024" | bc 2>/dev/null || echo "0")
            echo -e "  ${GRAY}$(basename "$file") ($size_mb MB)${NC}"
        else
            echo -e "  ${GRAY}$(basename "$file")${NC}"
        fi
    done
fi

echo ""
echo -e "${GREEN}构建完成！${NC}"

# 设置可执行权限 (仅对当前平台的可执行文件)
if [[ "$OSTYPE" == "darwin"* ]]; then
    # macOS
    find "$OUTPUT_DIR" -name "*osx*" -type f ! -name "*.tar.gz" -exec chmod +x {} \; 2>/dev/null || true
elif [[ "$OSTYPE" == "linux-gnu"* ]]; then
    # Linux
    find "$OUTPUT_DIR" -name "*linux*" -type f ! -name "*.tar.gz" -exec chmod +x {} \; 2>/dev/null || true
fi 