namespace Csanno.Attributes;

/// <summary>
/// 将拦截器绑定到指定的注解类型（泛型版本）
/// 标记此特性的类会被自动注册为组件，无需额外添加 [Component] 特性
/// </summary>
/// <typeparam name="TAttribute">要绑定的注解类型，必须继承自 BaseInterceptAttribute</typeparam>
public sealed class BindWithAttribute<TAttribute> : ComponentAttribute
    where TAttribute : BaseInterceptAttribute
{
}

/// <summary>
/// 将拦截器绑定到指定的注解类型（非泛型版本，用于运行时处理）
/// 标记此特性的类会被自动注册为组件，无需额外添加 [Component] 特性
/// </summary>
public sealed class BindWithAttribute : ComponentAttribute
{
    /// <summary>
    /// 获取绑定的注解类型
    /// </summary>
    public Type AttributeType { get; }

    /// <summary>
    /// 创建拦截器绑定特性
    /// </summary>
    /// <param name="attributeType">要绑定的注解类型，必须继承自 BaseInterceptAttribute</param>
    public BindWithAttribute(Type attributeType)
    {
        ArgumentNullException.ThrowIfNull(attributeType);
        if (!typeof(BaseInterceptAttribute).IsAssignableFrom(attributeType))
        {
            throw new ArgumentException(
                $"Type must inherit from {nameof(BaseInterceptAttribute)}.", 
                nameof(attributeType));
        }
        AttributeType = attributeType;
    }
}
