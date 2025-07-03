using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace subtitle_sync.Services;

/// <summary>
/// 高性能文件处理器，优化大文件读取和处理
/// </summary>
public static class FileProcessor
{
    private const int DefaultBufferSize = 65536; // 64KB
    private const int MaxFileSize = 50 * 1024 * 1024; // 50MB限制

    /// <summary>
    /// 高效读取文件内容，自动检测编码
    /// </summary>
    public static async Task<string> ReadFileContentAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"文件不存在: {filePath}");

        var fileInfo = new FileInfo(filePath);
        
        // 检查文件大小
        if (fileInfo.Length > MaxFileSize)
            throw new InvalidOperationException($"文件过大，超过{MaxFileSize / (1024 * 1024)}MB限制");

        // 小文件直接读取
        if (fileInfo.Length < 1024 * 1024) // 1MB以下
        {
            return await File.ReadAllTextAsync(filePath, Encoding.UTF8);
        }

        // 大文件分块读取
        return await ReadLargeFileAsync(filePath);
    }

    /// <summary>
    /// 分块读取大文件
    /// </summary>
    private static async Task<string> ReadLargeFileAsync(string filePath)
    {
        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize);
        using var reader = new StreamReader(fileStream, Encoding.UTF8, true, DefaultBufferSize);
        
        var stringBuilder = new StringBuilder();
        var buffer = new char[DefaultBufferSize];
        int charsRead;
        
        while ((charsRead = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            stringBuilder.Append(buffer, 0, charsRead);
        }
        
        return stringBuilder.ToString();
    }

    /// <summary>
    /// 高效写入文件
    /// </summary>
    public static async Task WriteFileContentAsync(string filePath, string content)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // 使用缓冲写入提升性能
        using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, DefaultBufferSize);
        using var writer = new StreamWriter(fileStream, Encoding.UTF8, DefaultBufferSize);
        
        await writer.WriteAsync(content);
        await writer.FlushAsync();
    }

    /// <summary>
    /// 检查文件是否为文本文件（避免处理二进制文件）
    /// </summary>
    public static async Task<bool> IsTextFileAsync(string filePath)
    {
        const int sampleSize = 8192; // 读取前8KB检测
        
        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var bufferSize = (int)Math.Min(sampleSize, fileStream.Length);
        var buffer = new byte[bufferSize];
        var totalRead = 0;
        
        while (totalRead < bufferSize)
        {
            var bytesRead = await fileStream.ReadAsync(buffer.AsMemory(totalRead, bufferSize - totalRead));
            if (bytesRead == 0) break;
            totalRead += bytesRead;
        }
        
        // 简单的文本检测：检查是否包含大量非打印字符
        int nonPrintableCount = 0;
        for (int i = 0; i < totalRead; i++)
        {
            byte b = buffer[i];
            if (b < 32 && b != 9 && b != 10 && b != 13) // 排除Tab、LF、CR
            {
                nonPrintableCount++;
            }
        }
        
        return totalRead == 0 || (double)nonPrintableCount / totalRead < 0.3; // 非打印字符少于30%认为是文本
    }
} 