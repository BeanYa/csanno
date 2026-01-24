namespace Csanno.Attributes;

/// <summary>
/// 原生方法调用类型，用于控制拦截器链中原生方法的调用行为
/// </summary>
public enum InvokeType
{
    /// <summary>
    /// 默认行为，等同于 WhenAllTrue
    /// </summary>
    Default = 0,

    /// <summary>
    /// 强制调用原生方法，忽略所有 OnBefore 返回值
    /// </summary>
    MustInvoke,

    /// <summary>
    /// 永不调用原生方法
    /// </summary>
    NeverInvoke,

    /// <summary>
    /// 所有 OnBefore 返回 true 时调用原生方法
    /// </summary>
    WhenAllTrue,

    /// <summary>
    /// 任一 OnBefore 返回 false 时调用原生方法
    /// </summary>
    WhenAnyFalse,

    /// <summary>
    /// 任一 OnBefore 返回 true 时调用原生方法
    /// </summary>
    WhenAnyTrue
}
