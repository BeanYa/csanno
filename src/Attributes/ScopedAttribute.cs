namespace Csanno.Attributes;

/// <summary>
/// 标记组件为 Scoped 生命周期（同一作用域内返回同一实例）
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ScopedAttribute : Attribute
{
}
