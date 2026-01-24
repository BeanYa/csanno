using System.Reflection;
using Csanno.Attributes;

namespace Csanno.Tests.Aop;

/// <summary>
/// 日志注解，用于标记需要记录日志的方法
/// </summary>
public class LoggingAttribute : BaseInterceptAttribute
{
    /// <summary>
    /// 附加信息
    /// </summary>
    public string AdditionalInfo { get; set; }

    /// <summary>
    /// 创建日志注解
    /// </summary>
    /// <param name="additionalInfo">附加信息</param>
    public LoggingAttribute(string additionalInfo = "")
    {
        AdditionalInfo = additionalInfo;
    }
}

/// <summary>
/// 日志拦截器，记录方法调用
/// </summary>
[BindWith<LoggingAttribute>]
public class LoggingInterceptor : BaseInterceptor
{
    /// <summary>
    /// 日志记录列表（用于测试验证）
    /// </summary>
    public static List<string> Logs { get; } = [];

    /// <summary>
    /// 清除日志
    /// </summary>
    public static void ClearLogs() => Logs.Clear();

    /// <inheritdoc />
    public override bool OnBefore(MethodInfo method, object?[] args, InvokeResult invokeResult)
    {
        Logs.Add($"[Before] {method.Name}({string.Join(", ", args)})");
        return true;
    }

    /// <inheritdoc />
    public override void OnAfter(MethodInfo method, object? result)
    {
        Logs.Add($"[After] {method.Name} => {result}");
    }
}

/// <summary>
/// 第二个日志拦截器，用于测试多拦截器场景
/// </summary>
[BindWith<LoggingAttribute>]
public class LoggingInterceptor2 : BaseInterceptor
{
    /// <summary>
    /// 日志记录列表（用于测试验证）
    /// </summary>
    public static List<string> Logs { get; } = [];

    /// <summary>
    /// 清除日志
    /// </summary>
    public static void ClearLogs() => Logs.Clear();

    /// <inheritdoc />
    public override bool OnBefore(MethodInfo method, object?[] args, InvokeResult invokeResult)
    {
        Logs.Add($"[Before-2] {method.Name}({string.Join(", ", args)})");
        return true;
    }

    /// <inheritdoc />
    public override void OnAfter(MethodInfo method, object? result)
    {
        Logs.Add($"[After-2] {method.Name} => {result}");
    }
}

/// <summary>
/// 缓存注解，用于标记需要缓存结果的方法
/// </summary>
public class CachingAttribute : BaseInterceptAttribute
{
    /// <summary>
    /// 缓存过期时间（秒）
    /// </summary>
    public int ExpirationSeconds { get; set; }

    /// <summary>
    /// 创建缓存注解
    /// </summary>
    /// <param name="expirationSeconds">缓存过期时间（秒），默认60秒</param>
    public CachingAttribute(int expirationSeconds = 60)
    {
        ExpirationSeconds = expirationSeconds;
    }
}

/// <summary>
/// 缓存拦截器，缓存方法调用结果
/// </summary>
[BindWith<CachingAttribute>]
public class CacheInterceptor : BaseInterceptor
{
    /// <summary>
    /// 缓存存储（方法名+参数 -> 结果）
    /// </summary>
    private static readonly Dictionary<string, object?> _cache = new();

    /// <summary>
    /// 当前调用的缓存键（用于在 OnBefore 和 OnAfter 之间传递）
    /// </summary>
    [ThreadStatic]
    private static string? _currentKey;

    /// <summary>
    /// 缓存命中次数（用于测试验证）
    /// </summary>
    public static int CacheHitCount { get; private set; }

    /// <summary>
    /// 缓存未命中次数（用于测试验证）
    /// </summary>
    public static int CacheMissCount { get; private set; }

    /// <summary>
    /// 清除缓存和计数器
    /// </summary>
    public static void ClearCache()
    {
        _cache.Clear();
        CacheHitCount = 0;
        CacheMissCount = 0;
    }

    /// <summary>
    /// 尝试从缓存获取结果
    /// </summary>
    public static bool TryGetCachedResult(string key, out object? result)
    {
        return _cache.TryGetValue(key, out result);
    }

    /// <summary>
    /// 生成缓存键
    /// </summary>
    private static string GenerateCacheKey(MethodInfo method, object?[] args)
    {
        return $"{method.DeclaringType?.Name}.{method.Name}({string.Join(",", args)})";
    }

    /// <inheritdoc />
    public override bool OnBefore(MethodInfo method, object?[] args, InvokeResult invokeResult)
    {
        var key = GenerateCacheKey(method, args);
        _currentKey = key;
        
        if (_cache.TryGetValue(key, out var cachedValue))
        {
            CacheHitCount++;
            invokeResult.SetValue(cachedValue);
        }
        else
        {
            CacheMissCount++;
        }
        return true;
    }

    /// <inheritdoc />
    public override void OnAfter(MethodInfo method, object? result)
    {
        if (_currentKey != null && !_cache.ContainsKey(_currentKey))
        {
            _cache[_currentKey] = result;
        }
    }
}

/// <summary>
/// 调用链测试注解
/// </summary>
public class ChainTestAttribute : BaseInterceptAttribute
{
}

/// <summary>
/// 调用链顺序记录拦截器（返回 true）
/// </summary>
[BindWith<ChainTestAttribute>]
public class ChainInterceptor1 : BaseInterceptor
{
    /// <summary>
    /// 调用顺序记录列表
    /// </summary>
    public static List<string> CallOrder { get; } = [];

    /// <summary>
    /// 清除记录
    /// </summary>
    public static void Clear() => CallOrder.Clear();

    /// <inheritdoc />
    public override bool OnBefore(MethodInfo method, object?[] args, InvokeResult invokeResult)
    {
        CallOrder.Add("I1.OnBefore");
        return true;
    }

    /// <inheritdoc />
    public override void OnAfter(MethodInfo method, object? result)
    {
        CallOrder.Add("I1.OnAfter");
    }
}

/// <summary>
/// 调用链顺序记录拦截器（返回 false，阻止原生方法执行）
/// </summary>
[BindWith<ChainTestAttribute>]
public class ChainInterceptor2 : BaseInterceptor
{
    /// <summary>
    /// 是否返回 true（允许调用原生方法）
    /// </summary>
    public static bool ShouldContinue { get; set; } = true;

    /// <inheritdoc />
    public override bool OnBefore(MethodInfo method, object?[] args, InvokeResult invokeResult)
    {
        ChainInterceptor1.CallOrder.Add("I2.OnBefore");
        return ShouldContinue;
    }

    /// <inheritdoc />
    public override void OnAfter(MethodInfo method, object? result)
    {
        ChainInterceptor1.CallOrder.Add("I2.OnAfter");
    }
}

/// <summary>
/// 调用链顺序记录拦截器（MustInvoke 模式）
/// </summary>
[BindWith<ChainTestAttribute>(InvokeType = InvokeType.MustInvoke)]
public class ChainInterceptor3MustInvoke : BaseInterceptor
{
    /// <inheritdoc />
    public override bool OnBefore(MethodInfo method, object?[] args, InvokeResult invokeResult)
    {
        ChainInterceptor1.CallOrder.Add("I3.OnBefore");
        return true;
    }

    /// <inheritdoc />
    public override void OnAfter(MethodInfo method, object? result)
    {
        ChainInterceptor1.CallOrder.Add("I3.OnAfter");
    }
}
