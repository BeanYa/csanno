namespace Csanno.Attributes;

/// <summary>
/// 标记组件为 PerMatchingLifetimeScope 生命周期
/// （在匹配指定标签的生命周期作用域内返回同一实例）
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class PerMatchingLifetimeScopeAttribute : Attribute
{
    /// <summary>
    /// 获取生命周期作用域的匹配标签
    /// </summary>
    public string[] Tags { get; }

    /// <summary>
    /// 初始化 <see cref="PerMatchingLifetimeScopeAttribute"/> 的新实例
    /// </summary>
    /// <param name="tags">生命周期作用域的匹配标签</param>
    /// <exception cref="ArgumentException">当标签为空或包含空值时抛出</exception>
    public PerMatchingLifetimeScopeAttribute(params string[] tags)
    {
        if (tags is null || tags.Length == 0)
        {
            throw new ArgumentException("至少需要一个标签", nameof(tags));
        }

        Tags = tags;
    }
}
