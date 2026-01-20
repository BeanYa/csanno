using NUnit.Framework;
using Autofac;

namespace Csanno.Tests;

/// <summary>
/// Transient 生命周期测试
/// </summary>
[TestFixture]
public class TransientLifetimeTests
{
    /// <summary>
    /// 测试用例：Transient 生命周期 - 每次解析返回新实例
    /// </summary>
    [Test]
    public void Transient_Lifetime_Should_Return_New_Instance_Each_Time()
    {
        // Arrange
        var container = ContainerFixture.CreateContainer();

        // Act
        var instance1 = container.Resolve<TransientComponent>();
        var instance2 = container.Resolve<TransientComponent>();

        // Assert
        Assert.That(instance1, Is.Not.SameAs(instance2));
    }
}
