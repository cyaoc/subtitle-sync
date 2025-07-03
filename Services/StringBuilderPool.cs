using System;
using System.Collections.Concurrent;
using System.Text;

namespace subtitle_sync.Services;

/// <summary>
/// StringBuilder对象池，提升字符串处理性能
/// </summary>
public static class StringBuilderPool
{
    private static readonly ConcurrentBag<StringBuilder> _pool = new();
    private const int MaxPoolSize = 10;
    private const int DefaultCapacity = 16384; // 16KB初始容量

    /// <summary>
    /// 从池中获取StringBuilder
    /// </summary>
    public static StringBuilder Get()
    {
        if (_pool.TryTake(out var sb))
        {
            sb.Clear();
            return sb;
        }
        
        return new StringBuilder(DefaultCapacity);
    }

    /// <summary>
    /// 归还StringBuilder到池中
    /// </summary>
    public static void Return(StringBuilder sb)
    {
        if (sb == null) return;
        
        // 如果StringBuilder太大，不回收以避免内存浪费
        if (sb.Capacity > DefaultCapacity * 4) return;
        
        // 限制池大小
        if (_pool.Count < MaxPoolSize)
        {
            _pool.Add(sb);
        }
    }

    /// <summary>
    /// 使用StringBuilder的便捷方法
    /// </summary>
    public static string Build(Action<StringBuilder> buildAction)
    {
        var sb = Get();
        try
        {
            buildAction(sb);
            return sb.ToString();
        }
        finally
        {
            Return(sb);
        }
    }
} 