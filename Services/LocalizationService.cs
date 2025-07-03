using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace subtitle_sync.Services;

public class LocalizationService : ILocalizationService
{
    private readonly IConfigurationService _configurationService;
    private SupportedLanguage _currentLanguage = SupportedLanguage.English;
    private readonly Dictionary<string, Dictionary<SupportedLanguage, string>> _translations;

    public LocalizationService(IConfigurationService configurationService)
    {
        _configurationService = configurationService;
        _translations = new Dictionary<string, Dictionary<SupportedLanguage, string>>
        {
            // 应用标题
            ["AppTitle"] = new()
            {
                [SupportedLanguage.English] = "Subtitle Converter",
                [SupportedLanguage.Chinese] = "字幕转换工具",
                [SupportedLanguage.Japanese] = "字幕変換ツール"
            },
            
            // 状态消息
            ["Ready"] = new()
            {
                [SupportedLanguage.English] = "Ready",
                [SupportedLanguage.Chinese] = "就绪",
                [SupportedLanguage.Japanese] = "準備完了"
            },
            
            ["SelectFile"] = new()
            {
                [SupportedLanguage.English] = "Please select a subtitle file",
                [SupportedLanguage.Chinese] = "请选择字幕文件",
                [SupportedLanguage.Japanese] = "字幕ファイルを選択してください"
            },
            
            ["Validating"] = new()
            {
                [SupportedLanguage.English] = "Validating file...",
                [SupportedLanguage.Chinese] = "正在验证文件...",
                [SupportedLanguage.Japanese] = "ファイルを検証中..."
            },
            
            ["Converting"] = new()
            {
                [SupportedLanguage.English] = "Converting...",
                [SupportedLanguage.Chinese] = "正在转换...",
                [SupportedLanguage.Japanese] = "変換中..."
            },
            
            ["ConvertComplete"] = new()
            {
                [SupportedLanguage.English] = "Conversion completed!",
                [SupportedLanguage.Chinese] = "转换完成！",
                [SupportedLanguage.Japanese] = "変換完了！"
            },
            
            ["ConvertFailed"] = new()
            {
                [SupportedLanguage.English] = "Conversion failed!",
                [SupportedLanguage.Chinese] = "转换失败！",
                [SupportedLanguage.Japanese] = "変換失敗！"
            },
            
            // UI元素
            ["DragHere"] = new()
            {
                [SupportedLanguage.English] = "Drag subtitle files here",
                [SupportedLanguage.Chinese] = "拖拽字幕文件到此处",
                [SupportedLanguage.Japanese] = "字幕ファイルをここにドラッグ"
            },
            
            ["SupportedFormats"] = new()
            {
                [SupportedLanguage.English] = "Supports SRT, VTT, ASS formats",
                [SupportedLanguage.Chinese] = "支持 SRT、VTT、ASS 格式",
                [SupportedLanguage.Japanese] = "SRT、VTT、ASS形式をサポート"
            },
            
            ["SelectFileButton"] = new()
            {
                [SupportedLanguage.English] = "Select File...",
                [SupportedLanguage.Chinese] = "选择文件...",
                [SupportedLanguage.Japanese] = "ファイル選択..."
            },
            
            ["InputFormat"] = new()
            {
                [SupportedLanguage.English] = "Input Format",
                [SupportedLanguage.Chinese] = "输入格式",
                [SupportedLanguage.Japanese] = "入力形式"
            },
            
            ["OutputFormat"] = new()
            {
                [SupportedLanguage.English] = "Output Format",
                [SupportedLanguage.Chinese] = "输出格式",
                [SupportedLanguage.Japanese] = "出力形式"
            },
            
            ["StartConvert"] = new()
            {
                [SupportedLanguage.English] = "Start Conversion",
                [SupportedLanguage.Chinese] = "开始转换",
                [SupportedLanguage.Japanese] = "変換開始"
            },
            
            ["Converting"] = new()
            {
                [SupportedLanguage.English] = "Converting...",
                [SupportedLanguage.Chinese] = "正在转换中...",
                [SupportedLanguage.Japanese] = "変換中..."
            },
            
            ["Tips"] = new()
            {
                [SupportedLanguage.English] = "💡 Tips",
                [SupportedLanguage.Chinese] = "💡 使用提示",
                [SupportedLanguage.Japanese] = "💡 ヒント"
            },
            
            ["Tip1"] = new()
            {
                [SupportedLanguage.English] = "• Batch processing multiple files",
                [SupportedLanguage.Chinese] = "• 支持批量处理多个文件",
                [SupportedLanguage.Japanese] = "• 複数ファイルの一括処理対応"
            },
            
            ["Tip2"] = new()
            {
                [SupportedLanguage.English] = "• Auto-skip same format files",
                [SupportedLanguage.Chinese] = "• 自动跳过相同格式文件",
                [SupportedLanguage.Japanese] = "• 同一形式ファイルの自動スキップ"
            },
            
            ["Tip3"] = new()
            {
                [SupportedLanguage.English] = "• Preserve original timing accuracy",
                [SupportedLanguage.Chinese] = "• 保持原有时间轴精度",
                [SupportedLanguage.Japanese] = "• 元のタイミング精度を保持"
            },
            
            ["UnknownFormat"] = new()
            {
                [SupportedLanguage.English] = "Unknown Format",
                [SupportedLanguage.Chinese] = "未知格式",
                [SupportedLanguage.Japanese] = "不明な形式"
            },
            
            ["Language"] = new()
            {
                [SupportedLanguage.English] = "Language",
                [SupportedLanguage.Chinese] = "语言",
                [SupportedLanguage.Japanese] = "言語"
            },
            
            // 多文件功能相关字符串
            ["SelectMultipleFiles"] = new()
            {
                [SupportedLanguage.English] = "Select Multiple Files",
                [SupportedLanguage.Chinese] = "选择多个文件",
                [SupportedLanguage.Japanese] = "複数ファイル選択"
            },
            
            ["AddFiles"] = new()
            {
                [SupportedLanguage.English] = "Add Files",
                [SupportedLanguage.Chinese] = "添加文件",
                [SupportedLanguage.Japanese] = "ファイル追加"
            },
            
            ["ClearAll"] = new()
            {
                [SupportedLanguage.English] = "Clear All",
                [SupportedLanguage.Chinese] = "清除所有",
                [SupportedLanguage.Japanese] = "すべてクリア"
            },
            
            ["BatchConvert"] = new()
            {
                [SupportedLanguage.English] = "Batch Convert",
                [SupportedLanguage.Chinese] = "批量转换",
                [SupportedLanguage.Japanese] = "一括変換"
            },
            
            ["FileCount"] = new()
            {
                [SupportedLanguage.English] = "files",
                [SupportedLanguage.Chinese] = "个文件",
                [SupportedLanguage.Japanese] = "ファイル"
            },
            
            ["Completed"] = new()
            {
                [SupportedLanguage.English] = "Completed",
                [SupportedLanguage.Chinese] = "已完成",
                [SupportedLanguage.Japanese] = "完了"
            },
            
            ["StatusPending"] = new()
            {
                [SupportedLanguage.English] = "Pending",
                [SupportedLanguage.Chinese] = "待转换",
                [SupportedLanguage.Japanese] = "待機中"
            },
            
            ["StatusConverting"] = new()
            {
                [SupportedLanguage.English] = "Converting...",
                [SupportedLanguage.Chinese] = "转换中...",
                [SupportedLanguage.Japanese] = "変換中..."
            },
            
            ["StatusCompleted"] = new()
            {
                [SupportedLanguage.English] = "Completed",
                [SupportedLanguage.Chinese] = "已完成",
                [SupportedLanguage.Japanese] = "完了"
            },
            
            ["StatusCopied"] = new()
            {
                [SupportedLanguage.English] = "Copied",
                [SupportedLanguage.Chinese] = "已复制",
                [SupportedLanguage.Japanese] = "コピー済み"
            },
            
            ["StatusFailed"] = new()
            {
                [SupportedLanguage.English] = "Failed",
                [SupportedLanguage.Chinese] = "转换失败",
                [SupportedLanguage.Japanese] = "失敗"
            },
            
            ["SkipReason"] = new()
            {
                [SupportedLanguage.English] = "Input and output formats are the same",
                [SupportedLanguage.Chinese] = "输入输出格式相同，跳过转换",
                [SupportedLanguage.Japanese] = "入出力形式が同じため、スキップ"
            },
            
            ["SelectOutputFolder"] = new()
            {
                [SupportedLanguage.English] = "Select Output Folder",
                [SupportedLanguage.Chinese] = "选择输出文件夹",
                [SupportedLanguage.Japanese] = "出力フォルダ選択"
            },
            
            ["BatchStarting"] = new()
            {
                [SupportedLanguage.English] = "Starting batch conversion for {0} files...",
                [SupportedLanguage.Chinese] = "开始批量转换 {0} 个文件...",
                [SupportedLanguage.Japanese] = "{0}ファイルの一括変換開始..."
            },
            
            ["BatchProgress"] = new()
            {
                [SupportedLanguage.English] = "Progress: {0}/{1}",
                [SupportedLanguage.Chinese] = "转换进度: {0}/{1}",
                [SupportedLanguage.Japanese] = "進行状況: {0}/{1}"
            },
            
            ["BatchComplete"] = new()
            {
                [SupportedLanguage.English] = "Batch conversion completed! Converted: {0}, Copied: {1}, Failed: {2}",
                [SupportedLanguage.Chinese] = "批量转换完成！已转换: {0}, 已复制: {1}, 失败: {2}",
                [SupportedLanguage.Japanese] = "一括変換完了！変換済み: {0}, コピー済み: {1}, 失敗: {2}"
            },
            
            ["FilesValidated"] = new()
            {
                [SupportedLanguage.English] = "Validated {0} files",
                [SupportedLanguage.Chinese] = "已验证 {0} 个文件",
                [SupportedLanguage.Japanese] = "{0}ファイルを検証完了"
            },
            
            ["SavedTo"] = new()
            {
                [SupportedLanguage.English] = "Saved to: {0}",
                [SupportedLanguage.Chinese] = "已保存到: {0}",
                [SupportedLanguage.Japanese] = "保存先: {0}"
            },
            
            ["CopiedTo"] = new()
            {
                [SupportedLanguage.English] = "Copied to: {0}",
                [SupportedLanguage.Chinese] = "已复制到: {0}",
                [SupportedLanguage.Japanese] = "コピー先: {0}"
            },
            
            // 验证消息
            ["FileNotFound"] = new()
            {
                [SupportedLanguage.English] = "File not found",
                [SupportedLanguage.Chinese] = "文件不存在",
                [SupportedLanguage.Japanese] = "ファイルが見つかりません"
            },
            
            ["UnrecognizedFormat"] = new()
            {
                [SupportedLanguage.English] = "Cannot recognize subtitle format",
                [SupportedLanguage.Chinese] = "无法识别字幕格式",
                [SupportedLanguage.Japanese] = "字幕形式を認識できません"
            },
            
            ["ParseFailed"] = new()
            {
                [SupportedLanguage.English] = "Failed to parse subtitle file",
                [SupportedLanguage.Chinese] = "解析字幕文件失败",
                [SupportedLanguage.Japanese] = "字幕ファイルの解析に失敗"
            },
            
            ["InvalidFormat"] = new()
            {
                [SupportedLanguage.English] = "Invalid subtitle file format",
                [SupportedLanguage.Chinese] = "字幕文件格式不正确",
                [SupportedLanguage.Japanese] = "字幕ファイル形式が正しくありません"
            },
            
            ["ValidationFailed"] = new()
            {
                [SupportedLanguage.English] = "Validation failed: {0}",
                [SupportedLanguage.Chinese] = "验证失败：{0}",
                [SupportedLanguage.Japanese] = "検証失敗：{0}"
            },
            
            ["ValidFileMessage"] = new()
            {
                [SupportedLanguage.English] = "Valid {0} file containing {1} subtitle entries",
                [SupportedLanguage.Chinese] = "有效的{0}文件，包含{1}个字幕条目",
                [SupportedLanguage.Japanese] = "有効な{0}ファイル、{1}個の字幕エントリを含む"
            }
        };

        // 从配置加载语言设置
        LoadLanguageFromConfiguration();
    }

    public SupportedLanguage CurrentLanguage
    {
        get => _currentLanguage;
        set
        {
            if (_currentLanguage != value)
            {
                _currentLanguage = value;
                OnPropertyChanged();
                // 通知所有翻译键都已更改
                OnPropertyChanged("Item[]");
                
                // 保存到配置文件
                SaveLanguageToConfiguration();
            }
        }
    }

    public string this[string key]
    {
        get
        {
            if (_translations.TryGetValue(key, out var translation))
            {
                if (translation.TryGetValue(_currentLanguage, out var value))
                {
                    return value;
                }
            }
            return key; // 如果找不到翻译，返回键本身
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public List<LanguageItem> GetSupportedLanguages()
    {
        return new List<LanguageItem>
        {
            new(SupportedLanguage.English, "English"),
            new(SupportedLanguage.Chinese, "中文"),
            new(SupportedLanguage.Japanese, "日本語")
        };
    }

    /// <summary>
    /// 从配置加载语言设置
    /// </summary>
    private void LoadLanguageFromConfiguration()
    {
        try
        {
            var config = _configurationService.Load();
            if (Enum.TryParse<SupportedLanguage>(config.Language, out var language))
            {
                _currentLanguage = language;
            }
        }
        catch
        {
            // 加载失败时使用默认语言
            _currentLanguage = SupportedLanguage.English;
        }
    }

    /// <summary>
    /// 保存语言设置到配置
    /// </summary>
    private void SaveLanguageToConfiguration()
    {
        try
        {
            var config = _configurationService.Load();
            config.Language = _currentLanguage.ToString();
            
            // 异步保存，避免阻塞UI
            _ = Task.Run(async () =>
            {
                await _configurationService.SaveAsync(config);
            });
        }
        catch
        {
            // 保存失败时静默处理
        }
    }
} 