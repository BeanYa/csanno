using System.Collections.Generic;

namespace Csanno.Generator.Models;

/// <summary>
/// 组件信息模型（编译期）
/// </summary>
internal sealed class ComponentInfo
{
    public string AssemblyName { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string FullTypeName { get; set; } = string.Empty;
    public InstanceLifetime Lifetime { get; set; }
    public List<ServiceInfo> Services { get; set; } = new List<ServiceInfo>();
    public List<MetadataInfo> Metadata { get; set; } = new List<MetadataInfo>();
    public string[]? LifetimeScopeTags { get; set; }
    public string? OwnedTypeName { get; set; }
}

/// <summary>
/// 组件实例生命周期类型
/// </summary>
internal enum InstanceLifetime
{
    /// <summary>
    /// 每次解析创建新实例
    /// </summary>
    Transient,

    /// <summary>
    /// 同一生命周期作用域内返回同一实例
    /// </summary>
    Scoped,

    /// <summary>
    /// 容器内全局单例
    /// </summary>
    Singleton,

    /// <summary>
    /// 每个 HTTP 请求内返回同一实例
    /// </summary>
    PerRequest,

    /// <summary>
    /// 在匹配指定标签的生命周期作用域内返回同一实例
    /// </summary>
    PerMatchingLifetimeScope,

    /// <summary>
    /// 作为拥有实例注册
    /// </summary>
    Owned,
}
