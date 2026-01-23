using Csanno.Attributes;

namespace Csanno.Tests.Aop;

/// <summary>
/// 示例服务类，用于测试 AOP 功能
/// </summary>
[Component]
public class SampleService
{
    /// <summary>
    /// 加法运算（带日志拦截）
    /// </summary>
    [Logging("Sample method execution")]
    public virtual int Add(int a, int b)
    {
        return a + b;
    }

    /// <summary>
    /// 问候方法（带日志拦截）
    /// </summary>
    [Logging]
    public virtual string Greet(string name)
    {
        return $"Hello, {name}!";
    }

    /// <summary>
    /// 无拦截注解的方法
    /// </summary>
    public virtual void NoInterception()
    {
        // 不会被拦截
    }

    /// <summary>
    /// 非 virtual 方法（不能被拦截）
    /// </summary>
    [Logging]
    public int NonVirtualMethod(int x)
    {
        return x * 2;
    }

    /// <summary>
    /// 模拟耗时计算（带缓存拦截）
    /// </summary>
    [Caching(expirationSeconds: 300)]
    public virtual int ExpensiveCalculation(int input)
    {
        // 模拟耗时操作
        return input * input;
    }
}

/// <summary>
/// 带构造函数参数的服务
/// </summary>
[Component]
public class ServiceWithDependency
{
    private readonly string _prefix;

    public ServiceWithDependency(string prefix)
    {
        _prefix = prefix;
    }

    [Logging]
    public virtual string Format(string message)
    {
        return $"{_prefix}: {message}";
    }
}
