namespace Csanno.Attributes;

/// <summary>
/// 影子特性定义 - 仅用于编译期类型识别
/// 这是 Source Generator 识别组件的标记特性
/// 实际的特性定义在主项目中
/// </summary>
internal sealed class ComponentAttribute : System.Attribute
{
    public System.Type? ServiceType { get; set; }
}

/// <summary>
/// 影子特性 - Transient 生命周期
/// </summary>
internal sealed class TransientAttribute : System.Attribute
{
}

/// <summary>
/// 影子特性 - Scoped 生命周期
/// </summary>
internal sealed class ScopedAttribute : System.Attribute
{
}

/// <summary>
/// 影子特性 - Singleton 生命周期
/// </summary>
internal sealed class SingletonAttribute : System.Attribute
{
}

/// <summary>
/// 影子特性 - PerRequest 生命周期
/// </summary>
internal sealed class PerRequestAttribute : System.Attribute
{
}

/// <summary>
/// 影子特性 - PerMatchingLifetimeScope 生命周期
/// </summary>
internal sealed class PerMatchingLifetimeScopeAttribute : System.Attribute
{
    public PerMatchingLifetimeScopeAttribute(params string[] tags)
    {
        Tags = tags;
    }

    public string[] Tags { get; }
}

/// <summary>
/// 影子特性 - Owned 生命周期
/// </summary>
internal sealed class OwnedAttribute : System.Attribute
{
    public System.Type? OwnedType { get; set; }
}

/// <summary>
/// 影子特性 - 服务类型映射
/// </summary>
internal sealed class AsServiceAttribute : System.Attribute
{
    public AsServiceAttribute(System.Type serviceType)
    {
        ServiceType = serviceType;
    }

    public System.Type ServiceType { get; }
}

/// <summary>
/// 影子特性 - 元数据
/// </summary>
internal sealed class WithMetadataAttribute : System.Attribute
{
    public WithMetadataAttribute(string key, object? value)
    {
        Key = key;
        Value = value;
    }

    public string Key { get; }
    public object? Value { get; }
}
