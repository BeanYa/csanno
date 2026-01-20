namespace Csanno.Internal;

/// <summary>
/// 组件注册信息
/// </summary>
/// <param name="ComponentType">组件类型</param>
/// <param name="Lifetime">生命周期</param>
/// <param name="ServiceTypes">服务类型集合</param>
/// <param name="Metadata">注册元数据字典</param>
/// <param name="LifetimeScopeTags">生命周期作用域标签（用于 PerMatchingLifetimeScope）</param>
/// <param name="OwnedType">拥有类型（用于 Owned 生命周期）</param>
internal record ComponentRegistration(
    Type ComponentType,
    InstanceLifetime Lifetime,
    Type[] ServiceTypes,
    IDictionary<string, object?>? Metadata = null,
    string[]? LifetimeScopeTags = null,
    Type? OwnedType = null
);
