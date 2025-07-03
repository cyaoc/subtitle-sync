using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using subtitle_sync.Models;
using subtitle_sync.Services;

namespace subtitle_sync;

/// <summary>
/// 字幕转换测试运行器
/// </summary>
public class SubtitleTestRunner
{
    private readonly SubtitleConverter _converter;
    private readonly List<TestResult> _results;

    public SubtitleTestRunner()
    {
        // 为测试创建本地化服务
        var configService = new ConfigurationService();
        var localizationService = new LocalizationService(configService);
        _converter = new SubtitleConverter(localizationService);
        _results = new List<TestResult>();
    }

    /// <summary>
    /// 运行所有测试
    /// </summary>
    public async Task RunAllTestsAsync()
    {
        Console.WriteLine("=== 字幕转换核心功能测试 ===");
        Console.WriteLine($"测试开始时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine();

        // 解析测试
        await RunParsingTestsAsync();
        
        // 转换测试 
        await RunConversionTestsAsync();
        
        // 边界情况测试
        await RunEdgeCaseTestsAsync();

        // 打印测试报告
        PrintTestReport();
    }

    /// <summary>
    /// 运行解析测试
    /// </summary>
    private async Task RunParsingTestsAsync()
    {
        Console.WriteLine("🔍 1. 解析测试");
        Console.WriteLine("================");

        // SRT解析测试
        await TestSrtParsing();
        
        // VTT解析测试
        await TestVttParsing();
        
        // ASS解析测试
        await TestAssParsing();

        Console.WriteLine();
    }

    /// <summary>
    /// 运行转换测试
    /// </summary>
    private async Task RunConversionTestsAsync()
    {
        Console.WriteLine("🔄 2. 转换测试");
        Console.WriteLine("================");

        var formats = new[] { SubtitleFormat.SRT, SubtitleFormat.VTT, SubtitleFormat.ASS };
        
        foreach (var sourceFormat in formats)
        {
            foreach (var targetFormat in formats)
            {
                if (sourceFormat != targetFormat)
                {
                    await TestConversion(sourceFormat, targetFormat);
                }
            }
        }

        Console.WriteLine();
    }

    /// <summary>
    /// 运行边界情况测试
    /// </summary>
    private async Task RunEdgeCaseTestsAsync()
    {
        Console.WriteLine("⚠️  3. 边界情况测试");
        Console.WriteLine("===================");

        await TestEmptyContent();
        await TestInvalidContent();
        await TestSpecialCharacters();
        await TestLongTexts();
        await TestShortTimes();

        Console.WriteLine();
    }

    #region SRT解析测试

    private async Task TestSrtParsing()
    {
        var testName = "SRT解析测试";
        try
        {
            var srtContent = GetSrtTemplate();
            var result = await _converter.ValidateFromContentAsync(srtContent, "test.srt");
            
            if (result.IsValid && result.Format == SubtitleFormat.SRT)
            {
                var subtitleFile = await _converter.ParseFromContentAsync(srtContent, "test.srt");
                var success = subtitleFile != null && subtitleFile.Count == 3;
                RecordTest(testName, success, success ? "解析成功，3个字幕条目" : "解析失败");
            }
            else
            {
                RecordTest(testName, false, result.Message);
            }
        }
        catch (Exception ex)
        {
            RecordTest(testName, false, $"异常: {ex.Message}");
        }
    }

    private string GetSrtTemplate()
    {
        return @"1
00:00:01,000 --> 00:00:03,000
Hello, this is a test subtitle.

2
00:00:04,500 --> 00:00:07,200
This is the second line with
multiple lines of text.

3
00:01:00,000 --> 00:01:02,500
Final subtitle with special chars: äöü &amp; éñ";
    }

    #endregion

    #region VTT解析测试

    private async Task TestVttParsing()
    {
        var testName = "VTT解析测试";
        try
        {
            var vttContent = GetVttTemplate();
            var result = await _converter.ValidateFromContentAsync(vttContent, "test.vtt");
            
            if (result.IsValid && result.Format == SubtitleFormat.VTT)
            {
                var subtitleFile = await _converter.ParseFromContentAsync(vttContent, "test.vtt");
                var success = subtitleFile != null && subtitleFile.Count == 3;
                RecordTest(testName, success, success ? "解析成功，3个字幕条目" : "解析失败");
            }
            else
            {
                RecordTest(testName, false, result.Message);
            }
        }
        catch (Exception ex)
        {
            RecordTest(testName, false, $"异常: {ex.Message}");
        }
    }

    private string GetVttTemplate()
    {
        return @"WEBVTT

00:00:01.000 --> 00:00:03.000
Hello, this is a VTT test subtitle.

00:00:04.500 --> 00:00:07.200
This is the second line with
multiple lines of text.

00:01:00.000 --> 00:01:02.500
Final VTT subtitle with special chars: äöü & éñ";
    }

    #endregion

    #region ASS解析测试

    private async Task TestAssParsing()
    {
        var testName = "ASS解析测试";
        try
        {
            var assContent = GetAssTemplate();
            var result = await _converter.ValidateFromContentAsync(assContent, "test.ass");
            
            if (result.IsValid && result.Format == SubtitleFormat.ASS)
            {
                var subtitleFile = await _converter.ParseFromContentAsync(assContent, "test.ass");
                var success = subtitleFile != null && subtitleFile.Count == 3;
                RecordTest(testName, success, success ? "解析成功，3个字幕条目" : "解析失败");
            }
            else
            {
                RecordTest(testName, false, result.Message);
            }
        }
        catch (Exception ex)
        {
            RecordTest(testName, false, $"异常: {ex.Message}");
        }
    }

    private string GetAssTemplate()
    {
        return @"[Script Info]
Title: Test Subtitle
ScriptType: v4.00+

[V4+ Styles]
Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding
Style: Default,Arial,20,&H00FFFFFF,&H000000FF,&H00000000,&H80000000,0,0,0,0,100,100,0,0,1,2,0,2,10,10,10,1

[Events]
Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text
Dialogue: 0,0:00:01.00,0:00:03.00,Default,,0,0,0,,Hello, this is an ASS test subtitle.
Dialogue: 0,0:00:04.50,0:00:07.20,Default,,0,0,0,,This is the second line with\Nmultiple lines of text.
Dialogue: 0,0:01:00.00,0:01:02.50,Default,,0,0,0,,Final ASS subtitle with {\i1}italic{\i0} and {\b1}bold{\b0} text.";
    }

    #endregion

    #region 转换测试

    private async Task TestConversion(SubtitleFormat sourceFormat, SubtitleFormat targetFormat)
    {
        var testName = $"{sourceFormat} → {targetFormat} 转换";
        try
        {
            var sourceContent = GetTemplateByFormat(sourceFormat);
            var convertedContent = await _converter.ConvertFromContentAsync(sourceContent, targetFormat, $"test.{sourceFormat.ToString().ToLower()}");
            
            if (!string.IsNullOrEmpty(convertedContent))
            {
                // 验证转换后的内容
                var validationResult = await _converter.ValidateFromContentAsync(convertedContent, $"converted.{targetFormat.ToString().ToLower()}");
                var success = validationResult.IsValid && validationResult.Format == targetFormat;
                RecordTest(testName, success, success ? "转换成功" : validationResult.Message);
            }
            else
            {
                RecordTest(testName, false, "转换结果为空");
            }
        }
        catch (Exception ex)
        {
            RecordTest(testName, false, $"异常: {ex.Message}");
        }
    }

    private string GetTemplateByFormat(SubtitleFormat format)
    {
        return format switch
        {
            SubtitleFormat.SRT => GetSrtTemplate(),
            SubtitleFormat.VTT => GetVttTemplate(),
            SubtitleFormat.ASS => GetAssTemplate(),
            _ => throw new ArgumentException($"不支持的格式: {format}")
        };
    }

    #endregion

    #region 边界情况测试

    private async Task TestEmptyContent()
    {
        var testName = "空内容测试";
        try
        {
            var result = await _converter.ValidateFromContentAsync("", "empty.srt");
            var success = !result.IsValid;
            RecordTest(testName, success, success ? "正确识别为无效内容" : "错误：空内容被识别为有效");
        }
        catch (Exception ex)
        {
            RecordTest(testName, false, $"异常: {ex.Message}");
        }
    }

    private async Task TestInvalidContent()
    {
        var testName = "无效内容测试";
        try
        {
            var invalidContent = "This is not a subtitle file at all!";
            var result = await _converter.ValidateFromContentAsync(invalidContent, "invalid.srt");
            var success = !result.IsValid;
            RecordTest(testName, success, success ? "正确识别为无效内容" : "错误：无效内容被识别为有效");
        }
        catch (Exception ex)
        {
            RecordTest(testName, false, $"异常: {ex.Message}");
        }
    }

    private async Task TestSpecialCharacters()
    {
        var testName = "特殊字符测试";
        try
        {
            var specialSrt = @"1
00:00:01,000 --> 00:00:03,000
测试中文字幕 🎬 emoji 表情

2
00:00:04,000 --> 00:00:06,000
日本語字幕テスト with ñ ü ä special chars

3
00:00:07,000 --> 00:00:09,000
Русский текст и символы: ""quotes"" & <tags>";

            var result = await _converter.ValidateFromContentAsync(specialSrt, "special.srt");
            var success = result.IsValid;
            RecordTest(testName, success, success ? "特殊字符处理成功" : result.Message);
        }
        catch (Exception ex)
        {
            RecordTest(testName, false, $"异常: {ex.Message}");
        }
    }

    private async Task TestLongTexts()
    {
        var testName = "长文本测试";
        try
        {
            var longText = string.Join(" ", Enumerable.Repeat("This is a very long subtitle text that should be handled properly by the parser.", 10));
            var longSrt = $@"1
00:00:01,000 --> 00:00:10,000
{longText}";

            var result = await _converter.ValidateFromContentAsync(longSrt, "long.srt");
            var success = result.IsValid;
            RecordTest(testName, success, success ? "长文本处理成功" : result.Message);
        }
        catch (Exception ex)
        {
            RecordTest(testName, false, $"异常: {ex.Message}");
        }
    }

    private async Task TestShortTimes()
    {
        var testName = "极短时间测试";
        try
        {
            var shortTimeSrt = @"1
00:00:00,001 --> 00:00:00,100
Flash text

2
23:59:59,900 --> 23:59:59,999
End of day text";

            var result = await _converter.ValidateFromContentAsync(shortTimeSrt, "short_time.srt");
            var success = result.IsValid;
            RecordTest(testName, success, success ? "极短时间处理成功" : result.Message);
        }
        catch (Exception ex)
        {
            RecordTest(testName, false, $"异常: {ex.Message}");
        }
    }

    #endregion

    #region 测试结果管理

    private void RecordTest(string testName, bool success, string message)
    {
        var result = new TestResult
        {
            TestName = testName,
            Success = success,
            Message = message,
            Timestamp = DateTime.Now
        };
        
        _results.Add(result);
        
        var status = success ? "✅ PASS" : "❌ FAIL";
        Console.WriteLine($"{status} | {testName}: {message}");
    }

    private void PrintTestReport()
    {
        Console.WriteLine("📊 测试报告");
        Console.WriteLine("=============");
        
        var totalTests = _results.Count;
        var passedTests = _results.Count(r => r.Success);
        var failedTests = totalTests - passedTests;
        var successRate = totalTests > 0 ? (double)passedTests / totalTests * 100 : 0;

        Console.WriteLine($"总测试数: {totalTests}");
        Console.WriteLine($"通过: {passedTests}");
        Console.WriteLine($"失败: {failedTests}");
        Console.WriteLine($"成功率: {successRate:F1}%");
        Console.WriteLine();

        if (failedTests > 0)
        {
            Console.WriteLine("❌ 失败的测试:");
            foreach (var failedTest in _results.Where(r => !r.Success))
            {
                Console.WriteLine($"  - {failedTest.TestName}: {failedTest.Message}");
            }
        }
        else
        {
            Console.WriteLine("🎉 所有测试都通过了！");
        }
        
        Console.WriteLine();
        Console.WriteLine($"测试完成时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
    }

    #endregion

    /// <summary>
    /// 测试结果类
    /// </summary>
    private class TestResult
    {
        public string TestName { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
} 