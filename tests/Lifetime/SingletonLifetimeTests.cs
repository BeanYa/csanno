using NUnit.Framework;
using Autofac;

namespace Csanno.Tests;

/// <summary>
/// Singleton 生命周期测试
/// </summary>
[TestFixture]
public class SingletonLifetimeTests
{
    /// <summary>
    /// 测试用例：Singleton 生命周期 - 容器内全局单例
    /// </summary>
    [Test]
    public void Singleton_Lifetime_Should_Return_Same_Instance_Globally()
    {
        // Arrange
        var container = ContainerFixture.CreateContainer();

        // Act
        using var scope1 = container.BeginLifetimeScope();
        using var scope2 = container.BeginLifetimeScope();
        var instance1 = scope1.Resolve<SingletonComponent>();
        var instance2 = scope2.Resolve<SingletonComponent>();

        // Assert
        Assert.That(instance1, Is.SameAs(instance2));
    }
}
