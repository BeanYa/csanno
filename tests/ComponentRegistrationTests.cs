using NUnit.Framework;
using Autofac;
using Autofac.Features.Metadata;
using Autofac.Features.OwnedInstances;
using Csanno.Attributes;

namespace Csanno.Tests;

/// <summary>
/// 组件注册集成测试
/// </summary>
[TestFixture]
public class ComponentRegistrationTests
{
    /// <summary>
    /// 测试用例：基础组件注册和解析
    /// </summary>
    [Test]
    public void Should_Register_And_Resolve_Basic_Component()
    {
        // Arrange
        var builder = new ContainerBuilder();
        builder.RegisterComponents(typeof(SimpleComponent).Assembly);

        // Act
        var container = builder.Build();
        var component = container.Resolve<SimpleComponent>();

        // Assert
        Assert.That(component, Is.Not.Null);
        Assert.That(component.Greet(), Is.EqualTo("Hello from SimpleComponent"));
    }

    /// <summary>
    /// 测试用例：Transient 生命周期 - 每次解析返回新实例
    /// </summary>
    [Test]
    public void Transient_Lifetime_Should_Return_New_Instance_Each_Time()
    {
        // Arrange
        var builder = new ContainerBuilder();
        builder.RegisterComponents(typeof(TransientComponent).Assembly);
        var container = builder.Build();

        // Act
        var instance1 = container.Resolve<TransientComponent>();
        var instance2 = container.Resolve<TransientComponent>();

        // Assert
        Assert.That(instance1, Is.Not.SameAs(instance2));
    }

    /// <summary>
    /// 测试用例：Scoped 生命周期 - 同一作用域内返回同一实例
    /// </summary>
    [Test]
    public void Scoped_Lifetime_Should_Return_Same_Instance_Within_Scope()
    {
        // Arrange
        var builder = new ContainerBuilder();
        builder.RegisterComponents(typeof(ScopedComponent).Assembly);
        var container = builder.Build();

        // Act
        using var scope = container.BeginLifetimeScope();
        var instance1 = scope.Resolve<ScopedComponent>();
        var instance2 = scope.Resolve<ScopedComponent>();

        // Assert
        Assert.That(instance1, Is.SameAs(instance2));
    }

    /// <summary>
    /// 测试用例：Singleton 生命周期 - 容器内全局单例
    /// </summary>
    [Test]
    public void Singleton_Lifetime_Should_Return_Same_Instance_Globally()
    {
        // Arrange
        var builder = new ContainerBuilder();
        builder.RegisterComponents(typeof(SingletonComponent).Assembly);
        var container = builder.Build();

        // Act
        using var scope1 = container.BeginLifetimeScope();
        using var scope2 = container.BeginLifetimeScope();
        var instance1 = scope1.Resolve<SingletonComponent>();
        var instance2 = scope2.Resolve<SingletonComponent>();

        // Assert
        Assert.That(instance1, Is.SameAs(instance2));
    }

    /// <summary>
    /// 测试用例：服务接口映射
    /// </summary>
    [Test]
    public void Should_Register_Component_As_Interface()
    {
        // Arrange
        var builder = new ContainerBuilder();
        builder.RegisterComponents(typeof(ServiceWithInterface).Assembly);
        var container = builder.Build();

        // Act
        var service = container.Resolve<IService>();

        // Assert
        Assert.That(service, Is.InstanceOf<ServiceWithInterface>());
        Assert.That(service.DoWork(), Is.EqualTo("Work done"));
    }

    /// <summary>
    /// 测试用例：构造函数依赖注入
    /// </summary>
    [Test]
    public void Should_Resolve_Constructor_Dependencies()
    {
        // Arrange
        var builder = new ContainerBuilder();
        builder.RegisterComponents(typeof(Consumer).Assembly);
        var container = builder.Build();

        // Act
        var consumer = container.Resolve<Consumer>();

        // Assert
        Assert.That(consumer, Is.Not.Null);
        Assert.That(consumer.Service, Is.Not.Null);
    }

    /// <summary>
    /// 测试用例：嵌套依赖解析
    /// </summary>
    [Test]
    public void Should_Resolve_Nested_Dependencies()
    {
        // Arrange
        var builder = new ContainerBuilder();
        builder.RegisterComponents(typeof(TopLevelConsumer).Assembly);
        var container = builder.Build();

        // Act
        var consumer = container.Resolve<TopLevelConsumer>();

        // Assert
        Assert.That(consumer, Is.Not.Null);
        Assert.That(consumer.Consumer, Is.Not.Null);
        Assert.That(consumer.Consumer.Service, Is.Not.Null);
    }

    /// <summary>
    /// 测试用例：多个服务接口注册
    /// </summary>
    [Test]
    public void Should_Register_Component_As_Multiple_Services()
    {
        // Arrange
        var builder = new ContainerBuilder();
        builder.RegisterComponents(typeof(MultiServiceComponent).Assembly);
        var container = builder.Build();

        // Act
        var service1 = container.Resolve<IService1>();
        var service2 = container.Resolve<IService2>();

        // Assert
        Assert.That(service1, Is.InstanceOf<MultiServiceComponent>());
        Assert.That(service2, Is.InstanceOf<MultiServiceComponent>());
        // 注意：由于默认是 Transient 生命周期，解析不同的服务接口会创建新实例
        // 如果需要同一实例，需要使用 Scoped 或 Singleton
    }

    /// <summary>
    /// 测试用例：元数据注册和检索
    /// </summary>
    [Test]
    public void Should_Register_Component_With_Metadata()
    {
        // Arrange
        var builder = new ContainerBuilder();
        builder.RegisterComponents(typeof(ComponentWithMetadata).Assembly);
        var container = builder.Build();

        // Act - 使用 Meta<T> 强类型元数据来解析
        var meta = container.Resolve<Meta<IComponentWithMetadata>>();

        // Assert
        Assert.That(meta, Is.Not.Null);
        Assert.That(meta.Metadata.TryGetValue("Name", out var name), Is.True);
        Assert.That(name, Is.EqualTo("TestComponent"));
        Assert.That(meta.Metadata.TryGetValue("Version", out var version), Is.True);
        Assert.That(version, Is.EqualTo(1));
    }

    /// <summary>
    /// 测试用例：元数据支持多种值类型
    /// </summary>
    [Test]
    public void Should_Register_Component_With_Multiple_Metadata_Types()
    {
        // Arrange
        var builder = new ContainerBuilder();
        builder.RegisterComponents(typeof(ComponentWithMultipleMetadataTypes).Assembly);
        var container = builder.Build();

        // Act
        var meta = container.Resolve<Meta<IComponentWithMultipleMetadata>>();

        // Assert
        Assert.That(meta.Metadata["StringKey"], Is.EqualTo("string value"));
        Assert.That(meta.Metadata["IntKey"], Is.EqualTo(42));
        Assert.That(meta.Metadata["BoolKey"], Is.EqualTo(true));
    }

    /// <summary>
    /// 测试用例：Owned 实例注册和解析
    /// </summary>
    [Test]
    public void Should_Register_Owned_Component()
    {
        // Arrange
        var builder = new ContainerBuilder();
        builder.RegisterComponents(typeof(OwnedComponent).Assembly);
        var container = builder.Build();

        // Act
        using var scope = container.BeginLifetimeScope();
        var owned = scope.Resolve<Owned<OwnedComponent>>();

        // Assert
        Assert.That(owned, Is.Not.Null);
        Assert.That(owned.Value, Is.Not.Null);
    }

    /// <summary>
    /// 测试用例：Owned 实例释放后不应再使用
    /// </summary>
    [Test]
    public void Owned_Component_Should_Be_Disposed()
    {
        // Arrange
        var builder = new ContainerBuilder();
        builder.RegisterComponents(typeof(DisposableOwnedComponent).Assembly);
        var container = builder.Build();

        // Act
        DisposableOwnedComponent? instance = null;
        using (var owned = container.Resolve<Owned<DisposableOwnedComponent>>())
        {
            instance = owned.Value;
        }

        // Assert
        Assert.That(instance, Is.Not.Null);
        Assert.That(instance.IsDisposed, Is.True);
    }

    /// <summary>
    /// 测试用例：PerMatchingLifetimeScope 注册
    /// </summary>
    [Test]
    public void Should_Register_PerMatchingLifetimeScope_Component()
    {
        // Arrange
        var builder = new ContainerBuilder();
        builder.RegisterComponents(typeof(MatchingScopeComponent).Assembly);
        var container = builder.Build();

        // Act
        using var taggedScope = container.BeginLifetimeScope("request");
        var instance1 = taggedScope.Resolve<MatchingScopeComponent>();
        var instance2 = taggedScope.Resolve<MatchingScopeComponent>();

        // Assert
        Assert.That(instance1, Is.SameAs(instance2));
    }

    /// <summary>
    /// 测试用例：PerMatchingLifetimeScope 在不匹配的标签下应失败
    /// </summary>
    [Test]
    public void PerMatchingLifetimeScope_Should_Fail_With_Wrong_Tag()
    {
        // Arrange
        var builder = new ContainerBuilder();
        builder.RegisterComponents(typeof(MatchingScopeComponent).Assembly);
        var container = builder.Build();

        // Act & Assert
        using var untaggedScope = container.BeginLifetimeScope();
        Assert.Throws<Autofac.Core.DependencyResolutionException>(() =>
        {
            untaggedScope.Resolve<MatchingScopeComponent>();
        });
    }

    /// <summary>
    /// 测试用例：PerMatchingLifetimeScope 支持多个标签
    /// </summary>
    [Test]
    public void Should_Register_PerMatchingLifetimeScope_With_Multiple_Tags()
    {
        // Arrange
        var builder = new ContainerBuilder();
        builder.RegisterComponents(typeof(MultiTagScopeComponent).Assembly);
        var container = builder.Build();

        // Act & Assert - 两个标签都应该能解析
        using var scope1 = container.BeginLifetimeScope("tag1");
        var instance1 = scope1.Resolve<MultiTagScopeComponent>();
        Assert.That(instance1, Is.Not.Null);

        using var scope2 = container.BeginLifetimeScope("tag2");
        var instance2 = scope2.Resolve<MultiTagScopeComponent>();
        Assert.That(instance2, Is.Not.Null);
    }
}

// ============== 测试组件定义 ==============

[Component]
public class SimpleComponent
{
    public string Greet() => "Hello from SimpleComponent";
}

[Component]
[Transient]
public class TransientComponent;

[Component]
[Scoped]
public class ScopedComponent;

[Component]
[Singleton]
public class SingletonComponent;

public interface IService
{
    string DoWork();
}

[Component]
[AsService(typeof(IService))]
public class ServiceWithInterface : IService
{
    public string DoWork() => "Work done";
}

[Component]
public class Consumer
{
    public IService Service { get; }

    public Consumer(IService service)
    {
        Service = service;
    }
}

[Component]
public class TopLevelConsumer
{
    public Consumer Consumer { get; }

    public TopLevelConsumer(Consumer consumer)
    {
        Consumer = consumer;
    }
}

public interface IService1 { }
public interface IService2 { }

[Component]
[AsService(typeof(IService1))]
[AsService(typeof(IService2))]
public class MultiServiceComponent : IService1, IService2 { }

// ============== 元数据测试组件 ==============

public interface IComponentWithMetadata { }

[Component]
[AsService(typeof(IComponentWithMetadata))]
[WithMetadata("Name", "TestComponent")]
[WithMetadata("Version", 1)]
public class ComponentWithMetadata : IComponentWithMetadata { }

public interface IComponentWithMultipleMetadata { }

[Component]
[AsService(typeof(IComponentWithMultipleMetadata))]
[WithMetadata("StringKey", "string value")]
[WithMetadata("IntKey", 42)]
[WithMetadata("BoolKey", true)]
public class ComponentWithMultipleMetadataTypes : IComponentWithMultipleMetadata { }

// ============== Owned 实例测试组件 ==============

[Component]
[Owned]
public class OwnedComponent;

[Component]
[Owned]
public class DisposableOwnedComponent : IDisposable
{
    public bool IsDisposed { get; private set; }

    public void Dispose()
    {
        IsDisposed = true;
    }
}

// ============== PerMatchingLifetimeScope 测试组件 ==============

[Component]
[PerMatchingLifetimeScope("request")]
public class MatchingScopeComponent;

[Component]
[PerMatchingLifetimeScope("tag1", "tag2")]
public class MultiTagScopeComponent;
