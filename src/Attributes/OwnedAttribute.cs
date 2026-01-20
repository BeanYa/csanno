namespace Csanno.Attributes;

/// <summary>
/// 标记组件为 Owned 生命周期（作为拥有实例注册）
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class OwnedAttribute : Attribute
{
    /// <summary>
    /// 获取拥有类型
    /// </summary>
    public Type? OwnedType { get; set; }
}
