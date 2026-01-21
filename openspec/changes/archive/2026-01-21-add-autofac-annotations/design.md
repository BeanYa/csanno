# 设计文档：Autofac 注解式组件注册

## 架构概述

本设计采用基于特性 (Attributes) 的声明式组件注册模式，配合 Autofac 的容器扩展能力实现自动扫描和注册。

### 核心组件

```
┌─────────────────────────────────────────────────────────────┐
│                     用户代码 (User Code)                      │
│  [Component]                                                │
│  public class UserService { ... }                          │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                   Csanno.Public API                          │
│  - RegisterComponents(assembly) 扩展方法                     │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                 ComponentScanner (内部)                      │
│  - 扫描程序集中的类型                                        │
│  - 识别带特性的类                                            │
│  - 构建注册信息                                              │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    Autofac Container                         │
│  执行实际的组件注册                                           │
└─────────────────────────────────────────────────────────────┘
```

## 特性设计

### 1. Component 特性

标记类为可注册组件：

```csharp
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ComponentAttribute : Attribute
{
    public Type? ServiceType { get; set; }
    public object? Metadata { get; set; }
}
```

### 2. 生命周期特性

每个特性对应 Autofac 的一种生命周期：

| 特性 | Autofac 等价 |
|------|-------------|
| `[Transient]` | InstancePerDependency() |
| `[Scoped]` | InstancePerLifetimeScope() |
| `[Singleton]` | SingleInstance() |
| `[PerRequest]` | InstancePerRequest() |
| `[PerMatchingLifetimeScope]` | InstancePerMatchingLifetimeScope() |
| `[Owned]` | InstancePerOwned<T>() |

### 3. 服务注册特性

用于指定服务/接口映射：

```csharp
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class AsServiceAttribute : Attribute
{
    public Type ServiceType { get; }
    public AsServiceAttribute(Type serviceType) { ... }
}
```

## 扫描器设计

### ComponentScanner

```csharp
internal static class ComponentScanner
{
    public static IEnumerable<ComponentRegistration> Scan(IEnumerable<Assembly> assemblies)
    {
        // 1. 遍历程序集中的所有类型
        // 2. 筛选带 Component 特性的类型
        // 3. 解析生命周期特性
        // 4. 解析服务映射特性
        // 5. 返回注册信息
    }

    private static InstanceLifetime ResolveLifetime(Type type)
    {
        // 按优先级查找生命周期特性
        // 1. Transient
        // 2. Scoped
        // 3. Singleton
        // 4. PerRequest
        // 5. PerMatchingLifetimeScope
        // 6. Owned
        // 7. 默认 Transient
    }
}
```

### 注册信息模型

```csharp
internal record ComponentRegistration
{
    public Type ComponentType { get; init; }
    public InstanceLifetime Lifetime { get; init; }
    public Type[] ServiceTypes { get; init; }
    public object? Metadata { get; init; }
    public string[]? LifetimeScopeTags { get; init; }
    public Type? OwnedType { get; init; }
}

internal enum InstanceLifetime
{
    Transient,
    Scoped,
    Singleton,
    PerRequest,
    PerMatchingLifetimeScope,
    Owned
}
```

## API 设计

### 扩展方法

```csharp
public static class RegistrationExtensions
{
    /// <summary>
    /// 注册指定程序集中的所有带 [Component] 特性的组件
    /// </summary>
    public static ContainerBuilder RegisterComponents(
        this ContainerBuilder builder,
        params Assembly[] assemblies)

    /// <summary>
    /// 注册调用程序集中的所有带 [Component] 特性的组件
    /// </summary>
    public static ContainerBuilder RegisterComponents(
        this ContainerBuilder builder)

    /// <summary>
    /// 注册指定类型所在程序集中的所有带 [Component] 特性的组件
    /// </summary>
    public static ContainerBuilder RegisterComponentsFromType<T>(
        this ContainerBuilder builder)
}
```

## 使用示例

### 基础用法

```csharp
[Component]
public class UserService
{
    public string Greet(string name) => $"Hello, {name}";
}
```

### 指定服务接口

```csharp
public interface IUserService { }

[Component]
[AsService(typeof(IUserService)]
public class UserService : IUserService { }
```

### 指定生命周期

```csharp
[Component]
[Singleton]
public class CacheService { }

[Component]
[Scoped]
public class UserRepository { }
```

### 依赖注入

```csharp
[Component]
public class OrderService
{
    private readonly IUserService _userService;
    private readonly IRepository _repository;

    // 构造函数注入自动解析
    public OrderService(IUserService userService, IRepository repository)
    {
        _userService = userService;
        _repository = repository;
    }
}
```

### 容器构建

```csharp
var builder = new ContainerBuilder();
builder.RegisterComponents(Assembly.GetExecutingAssembly());

var container = builder.Build();
var userService = container.Resolve<IUserService>();
```

## 技术决策

### 为什么选择特性而不是源生成器？

1. **简单性**: 特性是 C# 原生支持，无需额外工具链
2. **调试友好**: 代码行为直观，易于调试
3. **兼容性**: 支持所有 .NET 版本和运行时
4. **运行时灵活性**: 可以动态决定扫描哪些程序集

### 生命周期特性优先级

当类上存在多个生命周期特性时，按以下优先级选择（优先级高的覆盖低的）：

1. Singleton
2. PerMatchingLifetimeScope
3. Scoped
4. PerRequest
5. Owned
6. Transient (默认)

### 服务类型解析策略

1. 如果指定了 `[AsService(typeof(T))]`，使用指定的类型
2. 如果 `ComponentAttribute.ServiceType` 有值，使用该类型
3. 否则，使用类本身作为服务类型

## 依赖项

- **Autofac** (~8.0): 依赖注入容器核心
- **System.Reflection**: 用于类型扫描
- **System.ComponentModel**: 用于特性基类

## 测试策略

1. **单元测试**: 测试各特性解析逻辑
2. **集成测试**: 测试完整的注册和解析流程
3. **边界测试**: 测试多个特性、无效输入等边界情况
4. **生命周期测试**: 验证不同生命周期的行为正确性
