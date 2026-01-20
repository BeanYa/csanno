using NUnit.Framework;
using Autofac;
using Csanno.Tests.Fixtures;
using Csanno.Tests.TestComponents.Dependencies;

namespace Csanno.Tests.Dependencies;

/// <summary>
/// 依赖注入测试
/// </summary>
[TestFixture]
public class DependencyInjectionTests
{
    /// <summary>
    /// 测试用例：构造函数依赖注入
    /// </summary>
    [Test]
    public void Should_Resolve_Constructor_Dependencies()
    {
        // Arrange
        var container = ContainerFixture.CreateContainer();

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
        var container = ContainerFixture.CreateContainer();

        // Act
        var consumer = container.Resolve<TopLevelConsumer>();

        // Assert
        Assert.That(consumer, Is.Not.Null);
        Assert.That(consumer.Consumer, Is.Not.Null);
        Assert.That(consumer.Consumer.Service, Is.Not.Null);
    }
}
