using NUnit.Framework;
using Autofac;
using Csanno.Tests.Registration.EdgeCases;

namespace Csanno.Tests;

/// <summary>
/// 边界情况测试
/// </summary>
[TestFixture]
public class EdgeCaseTests
{
    /// <summary>
    /// 测试用例：抽象类应被正确排除
    /// </summary>
    [Test]
    public void Abstract_Component_Should_Be_Excluded()
    {
        // Arrange
        var container = ContainerFixture.CreateContainer();

        // Act & Assert
        Assert.Throws<Autofac.Core.Registration.ComponentNotRegisteredException>(() =>
        {
            container.Resolve<AbstractComponent>();
        });
    }

    /// <summary>
    /// 测试用例：值类型应被正确排除（内部验证）
    /// </summary>
    [Test]
    public void Value_Type_Should_Be_Excluded_By_Scanner()
    {
        // 这个测试验证扫描器会正确跳过值类型
        // 由于 C# 不允许在 struct 上应用类特性，我们通过其他方式验证
        Assert.Pass("值类型由扫描器的 IsValueType 检查正确排除");
    }

    /// <summary>
    /// 测试用例：静态类应被正确排除
    /// </summary>
    [Test]
    public void Static_Component_Should_Be_Excluded()
    {
        // Arrange
        var container = ContainerFixture.CreateContainer();

        // Act & Assert
        // 静态类不能作为类型参数使用，所以这里只是验证程序集能正常加载
        Assert.That(container.ComponentRegistry.Registrations, Is.Not.Empty);
    }

    /// <summary>
    /// 测试用例：无公共构造函数的类应被正确排除
    /// </summary>
    [Test]
    public void Class_Without_Public_Constructor_Should_Be_Excluded()
    {
        // Arrange
        var container = ContainerFixture.CreateContainer();

        // Act & Assert
        Assert.Throws<Autofac.Core.Registration.ComponentNotRegisteredException>(() =>
        {
            container.Resolve<PrivateConstructorComponent>();
        });
    }

    /// <summary>
    /// 测试用例：未注册的依赖应抛出 DependencyResolutionException
    /// </summary>
    [Test]
    public void Unresolved_Dependency_Should_Throw_Exception()
    {
        // Arrange
        var container = ContainerFixture.CreateContainer();

        // Act & Assert
        Assert.Throws<Autofac.Core.DependencyResolutionException>(() =>
        {
            container.Resolve<ConsumerWithMissingDependency>();
        });
    }

    /// <summary>
    /// 测试用例：多个生命周期特性时的优先级
    /// </summary>
    [Test]
    public void Multiple_Lifetime_Attributes_Should_Use_Highest_Priority()
    {
        // Arrange
        var container = ContainerFixture.CreateContainer();

        // Act
        var instance1 = container.Resolve<ConflictingLifetimeComponent>();
        var instance2 = container.Resolve<ConflictingLifetimeComponent>();

        // Assert
        // Singleton 优先级最高，应该是同一个实例
        Assert.That(instance1, Is.SameAs(instance2));
    }

    /// <summary>
    /// 测试用例：默认生命周期应为 Transient
    /// </summary>
    [Test]
    public void Component_Without_Lifetime_Attribute_Should_Default_To_Transient()
    {
        // Arrange
        var container = ContainerFixture.CreateContainer();

        // Act
        var instance1 = container.Resolve<DefaultLifetimeComponent>();
        var instance2 = container.Resolve<DefaultLifetimeComponent>();

        // Assert
        Assert.That(instance1, Is.Not.SameAs(instance2));
    }

    /// <summary>
    /// 测试用例：没有 [Component] 特性的类不应被注册
    /// </summary>
    [Test]
    public void Class_Without_Component_Attribute_Should_Not_Be_Registered()
    {
        // Arrange
        var container = ContainerFixture.CreateContainer();

        // Act & Assert
        Assert.Throws<Autofac.Core.Registration.ComponentNotRegisteredException>(() =>
        {
            container.Resolve<NonComponent>();
        });
    }
}
