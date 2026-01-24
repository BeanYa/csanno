using NUnit.Framework;
using Autofac;

namespace Csanno.Tests;

/// <summary>
/// Scoped 生命周期测试
/// </summary>
[TestFixture]
public class ScopedLifetimeTests
{
    /// <summary>
    /// 测试用例：Scoped 生命周期 - 同一作用域内返回同一实例
    /// </summary>
    [Test]
    public void Scoped_Lifetime_Should_Return_Same_Instance_Within_Scope()
    {
        // Arrange
        var container = ContainerFixture.CreateContainer();

        // Act
        using var scope = container.BeginLifetimeScope();
        var instance1 = scope.Resolve<ScopedComponent>();
        var instance2 = scope.Resolve<ScopedComponent>();

        // Assert
        Assert.That(instance1, Is.SameAs(instance2));
    }
}
