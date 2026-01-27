using Csanno.Attributes;

namespace Csanno.Tests.Registration.Lifetime;

/// <summary>
/// 简单组件，用于基础注册测试
/// </summary>
[Component]
public class SimpleComponent
{
    public string Greet() => "Hello from SimpleComponent";
}

/// <summary>
/// Transient 生命周期组件
/// </summary>
[Component]
[Transient]
public class TransientComponent;

/// <summary>
/// Scoped 生命周期组件
/// </summary>
[Component]
[Scoped]
public class ScopedComponent;

/// <summary>
/// Singleton 生命周期组件
/// </summary>
[Component]
[Singleton]
public class SingletonComponent;
