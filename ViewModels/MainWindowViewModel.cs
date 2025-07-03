using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using subtitle_sync.Models;
using subtitle_sync.Services;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

namespace subtitle_sync.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly SubtitleConverter _converter;
    private readonly ILocalizationService _localization;
    private SubtitleFormatItem? _selectedOutputFormat;
    private string _statusMessage = string.Empty;
    private bool _isConverting = false;
    private LanguageItem? _selectedLanguage;
    private double _overallProgress = 0;

    public MainWindowViewModel(SubtitleConverter converter, ILocalizationService localizationService)
    {
        _converter = converter;
        _localization = localizationService;
        
        // 初始化命令
        SelectFilesCommand = new RelayCommand(async () => await SelectFilesAsync());
        ConvertCommand = new RelayCommand(async () => await ConvertAsync(), () => CanConvert());
        ClearFilesCommand = new RelayCommand(async () => { ClearFiles(); await Task.CompletedTask; });
        RemoveFileCommand = new RelayCommand<FileConversionItem>(RemoveFile);
        
        // 初始化支持的输出格式和语言
        SupportedOutputFormats = new ObservableCollection<SubtitleFormatItem>();
        SupportedLanguages = new ObservableCollection<LanguageItem>();
        ConversionItems = new ObservableCollection<FileConversionItem>();
        LoadSupportedFormats();
        LoadSupportedLanguages();
        
        // 订阅本地化服务的属性变更
        _localization.PropertyChanged += OnLocalizationChanged;
        
        // 设置初始状态消息
        StatusMessage = _localization["Ready"];
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    // 属性

    public SubtitleFormatItem? SelectedOutputFormat
    {
        get => _selectedOutputFormat;
        set
        {
            if (SetProperty(ref _selectedOutputFormat, value))
            {
                // 更新所有文件的输出格式
                if (value != null)
                {
                    foreach (var item in ConversionItems)
                    {
                        item.OutputFormat = value.Format;
                    }
                }
                ((RelayCommand)ConvertCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public bool IsConverting
    {
        get => _isConverting;
        set
        {
            if (SetProperty(ref _isConverting, value))
            {
                ((RelayCommand)ConvertCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public ObservableCollection<SubtitleFormatItem> SupportedOutputFormats { get; }
    public ObservableCollection<LanguageItem> SupportedLanguages { get; }
    public ObservableCollection<FileConversionItem> ConversionItems { get; }

    public LanguageItem? SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            if (SetProperty(ref _selectedLanguage, value) && value != null)
            {
                _localization.CurrentLanguage = value.Language;
            }
        }
    }



    public double OverallProgress
    {
        get => _overallProgress;
        set => SetProperty(ref _overallProgress, value);
    }

    public string FileCountInfo => $"{ConversionItems.Count} {_localization["FileCount"]}";
    public string CompletedFilesInfo => $"{ConversionItems.Count(x => x.Status == ConversionStatus.Completed || x.Status == ConversionStatus.Copied)} / {ConversionItems.Count} {_localization["Completed"]}";

    // 多语言绑定属性
    public string AppTitle => _localization["AppTitle"];
    public string DragHereText => _localization["DragHere"];
    public string SupportedFormatsText => _localization["SupportedFormats"];
    public string SelectFilesButtonText => _localization["SelectMultipleFiles"];
    public string OutputFormatText => _localization["OutputFormat"];
    public string StartConvertText => _localization["BatchConvert"];
    public string ConvertingText => _localization["Converting"];
    public string TipsText => _localization["Tips"];
    public string Tip1Text => _localization["Tip1"];
    public string Tip2Text => _localization["Tip2"];
    public string Tip3Text => _localization["Tip3"];
    public string LanguageText => _localization["Language"];
    public string AddFilesText => _localization["AddFiles"];
    public string ClearAllText => _localization["ClearAll"];

    // 命令
    public ICommand SelectFilesCommand { get; }
    public ICommand ConvertCommand { get; }
    public ICommand ClearFilesCommand { get; }
    public ICommand RemoveFileCommand { get; }

    // 拖拽支持
    public Task HandleDroppedFilesAsync(IEnumerable<IStorageItem> items)
    {
        var files = items.OfType<IStorageFile>().ToList();
        if (files.Count > 0)
        {
            // 统一使用多文件模式
            AddFiles(files.Select(f => f.Path.LocalPath).ToList());
        }
        return Task.CompletedTask;
    }

    private async Task SelectFilesAsync()
    {
        try
        {
            var topLevel = TopLevel.GetTopLevel(App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop 
                ? desktop.MainWindow : null);

            if (topLevel != null)
            {
                var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = _localization["SelectMultipleFiles"],
                    AllowMultiple = true,
                    FileTypeFilter = new[]
                    {
                        new FilePickerFileType("字幕文件")
                        {
                            Patterns = new[] { "*.srt", "*.vtt", "*.ass" }
                        },
                        FilePickerFileTypes.All
                    }
                });

                if (files.Count > 0)
                {
                    AddFiles(files.Select(f => f.Path.LocalPath).ToList());
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"选择文件失败：{ex.Message}";
        }
    }

    private async void AddFiles(List<string> filePaths)
    {
        // 添加文件到转换列表
        foreach (var filePath in filePaths)
        {
            if (!ConversionItems.Any(x => x.FilePath == filePath))
            {
                var item = new FileConversionItem(filePath);
                item.StatusText = _localization["StatusPending"];
                if (SelectedOutputFormat != null)
                {
                    item.OutputFormat = SelectedOutputFormat.Format;
                }
                ConversionItems.Add(item);
            }
        }
        
        UpdateFileInfo();
        
        // 异步验证所有新添加的文件
        await ValidateAllFilesAsync();
        
        // 验证完成后再次更新UI
        UpdateFileInfo();
    }

    private void ClearFiles()
    {
        ConversionItems.Clear();
        OverallProgress = 0;
        StatusMessage = _localization["Ready"];
        UpdateFileInfo();
    }

    private void RemoveFile(FileConversionItem? item)
    {
        if (item != null)
        {
            ConversionItems.Remove(item);
            UpdateFileInfo();
        }
    }

    private async Task ValidateAllFilesAsync()
    {
        foreach (var item in ConversionItems)
        {
            try
            {
                var (isValid, message, format) = await _converter.ValidateAsync(item.FilePath);
                item.InputFormat = format;
                item.Message = message;
                
                if (!isValid)
                {
                    item.Status = ConversionStatus.Failed;
                    item.StatusText = _localization["StatusFailed"];
                }
            }
            catch (Exception ex)
            {
                item.Status = ConversionStatus.Failed;
                item.StatusText = _localization["StatusFailed"];
                item.Message = string.Format(_localization["ValidationFailed"], ex.Message);
            }
        }
        
        StatusMessage = string.Format(_localization["FilesValidated"], ConversionItems.Count);
    }

    private void UpdateFileInfo()
    {
        OnPropertyChanged(nameof(FileCountInfo));
        OnPropertyChanged(nameof(CompletedFilesInfo));
        ((RelayCommand)ConvertCommand).RaiseCanExecuteChanged();
    }

    private async Task ConvertAsync()
    {
        if (!CanConvert()) return;

        try
        {
            IsConverting = true;
            await ConvertMultipleFilesAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"{_localization["ConvertFailed"]}: {ex.Message}";
        }
        finally
        {
            IsConverting = false;
            OverallProgress = 0;
        }
    }



    private async Task ConvertMultipleFilesAsync()
    {
        if (SelectedOutputFormat == null) return;

        var topLevel = TopLevel.GetTopLevel(App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop 
            ? desktop.MainWindow : null);

        if (topLevel == null) return;

        // 选择输出文件夹
        var folder = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = _localization["SelectOutputFolder"],
            AllowMultiple = false
        });

        if (folder.Count == 0) return;

        var outputFolder = folder[0].Path.LocalPath;
        var outputFormat = SelectedOutputFormat.Format;
        var extension = outputFormat.GetFileExtension();

        var itemsToProcess = ConversionItems.Where(item => 
            item.Status != ConversionStatus.Failed && 
            item.InputFormat != SubtitleFormat.Unknown).ToList();

        var totalItems = itemsToProcess.Count;
        var processedItems = 0;

        StatusMessage = string.Format(_localization["BatchStarting"], totalItems);

        foreach (var item in itemsToProcess)
        {
            try
            {
                item.OutputFormat = outputFormat;
                
                // 检查是否为相同格式（需要复制而不是转换）
                if (item.SameFormat)
                {
                    item.Status = ConversionStatus.Converting;
                    item.StatusText = _localization["StatusConverting"];
                    item.Message = _localization["StatusConverting"];

                    var outputFileName = Path.GetFileNameWithoutExtension(item.FileName) + extension;
                    var outputPath = Path.Combine(outputFolder, outputFileName);

                    // 复制文件到输出目录
                    File.Copy(item.FilePath, outputPath, true);
                    
                    item.Status = ConversionStatus.Copied;
                    item.StatusText = _localization["StatusCopied"];
                    item.Message = string.Format(_localization["CopiedTo"], outputFileName);
                    item.Progress = 100;
                }
                else
                {
                    item.Status = ConversionStatus.Converting;
                    item.StatusText = _localization["StatusConverting"];
                    item.Message = _localization["StatusConverting"];

                    var outputFileName = Path.GetFileNameWithoutExtension(item.FileName) + extension;
                    var outputPath = Path.Combine(outputFolder, outputFileName);

                    var success = await _converter.ConvertAndSaveAsync(
                        item.FilePath,
                        outputPath,
                        outputFormat);

                    if (success)
                    {
                        item.Status = ConversionStatus.Completed;
                        item.StatusText = _localization["StatusCompleted"];
                        item.Message = string.Format(_localization["SavedTo"], outputFileName);
                        item.Progress = 100;
                    }
                    else
                    {
                        item.Status = ConversionStatus.Failed;
                        item.StatusText = _localization["StatusFailed"];
                        item.Message = _localization["StatusFailed"];
                        item.Progress = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                item.Status = ConversionStatus.Failed;
                item.StatusText = _localization["StatusFailed"];
                item.Message = $"{_localization["StatusFailed"]}: {ex.Message}";
                item.Progress = 0;
            }

            processedItems++;
            OverallProgress = (double)processedItems / totalItems * 100;
            StatusMessage = string.Format(_localization["BatchProgress"], processedItems, totalItems);

            UpdateFileInfo();
        }

        var completedCount = ConversionItems.Count(x => x.Status == ConversionStatus.Completed);
        var copiedCount = ConversionItems.Count(x => x.Status == ConversionStatus.Copied);
        var failedCount = ConversionItems.Count(x => x.Status == ConversionStatus.Failed);

        StatusMessage = string.Format(_localization["BatchComplete"], completedCount, copiedCount, failedCount);
    }

    private bool CanConvert()
    {
        if (IsConverting || SelectedOutputFormat == null) return false;

        // 至少有一个有效文件（无论格式是否相同）
        return ConversionItems.Any(item => 
            item.InputFormat != SubtitleFormat.Unknown && 
            item.Status != ConversionStatus.Failed);
    }



    private void LoadSupportedFormats()
    {
        var formats = _converter.GetSupportedFormats();
        foreach (var format in formats)
        {
            SupportedOutputFormats.Add(new SubtitleFormatItem(format, format.GetDisplayName()));
        }
        
        // 设置默认选择为SRT
        SelectedOutputFormat = SupportedOutputFormats.FirstOrDefault(f => f.Format == SubtitleFormat.SRT);
    }

    private void LoadSupportedLanguages()
    {
        var languages = _localization.GetSupportedLanguages();
        foreach (var language in languages)
        {
            SupportedLanguages.Add(language);
        }
        
        // 设置默认语言为英文
        SelectedLanguage = SupportedLanguages.FirstOrDefault(l => l.Language == SupportedLanguage.English);
    }

    private void OnLocalizationChanged(object? sender, PropertyChangedEventArgs e)
    {
        // 当语言更改时，通知所有本地化属性
        OnPropertyChanged(nameof(AppTitle));
        OnPropertyChanged(nameof(DragHereText));
        OnPropertyChanged(nameof(SupportedFormatsText));
        OnPropertyChanged(nameof(SelectFilesButtonText));
        OnPropertyChanged(nameof(OutputFormatText));
        OnPropertyChanged(nameof(StartConvertText));
        OnPropertyChanged(nameof(ConvertingText));
        OnPropertyChanged(nameof(TipsText));
        OnPropertyChanged(nameof(Tip1Text));
        OnPropertyChanged(nameof(Tip2Text));
        OnPropertyChanged(nameof(Tip3Text));
        OnPropertyChanged(nameof(LanguageText));
        OnPropertyChanged(nameof(AddFilesText));
        OnPropertyChanged(nameof(ClearAllText));
        OnPropertyChanged(nameof(FileCountInfo));
        OnPropertyChanged(nameof(CompletedFilesInfo));
        
        // 更新状态消息
        if (ConversionItems.Count == 0)
        {
            StatusMessage = _localization["Ready"];
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}

// 格式选择项
public class SubtitleFormatItem
{
    public SubtitleFormat Format { get; }
    public string DisplayName { get; }

    public SubtitleFormatItem(SubtitleFormat format, string displayName)
    {
        Format = format;
        DisplayName = displayName;
    }

    public override string ToString() => DisplayName;
}

// 简单的命令实现
public class RelayCommand : ICommand
{
    private readonly Func<Task> _executeAsync;
    private readonly Func<bool>? _canExecute;

    public RelayCommand(Func<Task> executeAsync, Func<bool>? canExecute = null)
    {
        _executeAsync = executeAsync;
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

    public async void Execute(object? parameter)
    {
        if (CanExecute(parameter))
        {
            await _executeAsync();
        }
    }

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}

public class RelayCommand<T> : ICommand
{
    private readonly Action<T?> _execute;
    private readonly Func<T?, bool>? _canExecute;

    public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => _canExecute?.Invoke((T?)parameter) ?? true;

    public void Execute(object? parameter)
    {
        _execute((T?)parameter);
    }

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
} 