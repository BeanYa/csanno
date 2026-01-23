namespace Csanno.Attributes;

/// <summary>
/// 标记类为可注册的 Autofac 组件
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class ComponentAttribute : Attribute
{
    /// <summary>
    /// 获取或设置服务类型。如果未设置，使用类本身作为服务类型
    /// </summary>
    public Type? ServiceType { get; set; }
}
