using System.Reflection;

namespace Csanno.Attributes;

/// <summary>
/// 方法拦截器接口
/// </summary>
public interface IInterceptor
{
    /// <summary>
    /// 方法执行前调用
    /// </summary>
    /// <param name="method">被拦截的方法信息</param>
    /// <param name="args">方法参数</param>
    /// <returns>返回 true 表示允许执行原生方法，返回 false 表示阻止执行</returns>
    bool OnBefore(MethodInfo method, object?[] args);

    /// <summary>
    /// 方法执行后调用
    /// </summary>
    /// <param name="method">被拦截的方法信息</param>
    /// <param name="result">方法返回值（void 方法为 null）</param>
    void OnAfter(MethodInfo method, object? result);
}

/// <summary>
/// 拦截器基类，提供默认空实现
/// </summary>
public abstract class BaseInterceptor : IInterceptor
{
    /// <inheritdoc />
    public virtual bool OnBefore(MethodInfo method, object?[] args) => true;

    /// <inheritdoc />
    public virtual void OnAfter(MethodInfo method, object? result) { }
}
