using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace subtitle_sync.Models;

public enum ConversionStatus
{
    Pending,     // 待转换
    Converting,  // 转换中
    Completed,   // 已完成
    Copied,      // 已复制（输入输出格式相同）
    Failed       // 转换失败
}

public class FileConversionItem : INotifyPropertyChanged
{
    private ConversionStatus _status = ConversionStatus.Pending;
    private string _message = string.Empty;
    private double _progress = 0;

    private SubtitleFormat _inputFormat = SubtitleFormat.Unknown;
    private SubtitleFormat _outputFormat = SubtitleFormat.Unknown;

    public string FilePath { get; }
    public string FileName => Path.GetFileName(FilePath);
    
    public SubtitleFormat InputFormat 
    { 
        get => _inputFormat;
        set 
        {
            if (SetProperty(ref _inputFormat, value))
            {
                OnPropertyChanged(nameof(InputFormatDisplay));
            }
        }
    }
    
    public SubtitleFormat OutputFormat 
    { 
        get => _outputFormat;
        set 
        {
            if (SetProperty(ref _outputFormat, value))
            {
                OnPropertyChanged(nameof(OutputFormatDisplay));
            }
        }
    }
    
    public ConversionStatus Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }
    
    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }
    
    public double Progress
    {
        get => _progress;
        set => SetProperty(ref _progress, value);
    }

    public string StatusText { get; set; } = string.Empty;

    public string StatusColor => Status switch
    {
        ConversionStatus.Pending => "#FF999999",
        ConversionStatus.Converting => "#FF007ACC",
        ConversionStatus.Completed => "#FF28A745",
        ConversionStatus.Copied => "#FF17A2B8",       // 青蓝色，表示复制操作
        ConversionStatus.Failed => "#FFDC3545",
        _ => "#FF999999"
    };

    public bool SameFormat => InputFormat != SubtitleFormat.Unknown 
                              && OutputFormat != SubtitleFormat.Unknown 
                              && InputFormat == OutputFormat;

    // 显示友好的格式名称
    public string InputFormatDisplay => InputFormat.GetDisplayName();
    public string OutputFormatDisplay => OutputFormat.GetDisplayName();

    public FileConversionItem(string filePath)
    {
        FilePath = filePath;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
} 