using NUnit.Framework;
using Autofac;
using Csanno.Tests.Fixtures;
using Csanno.Tests.TestComponents.Owned;

namespace Csanno.Tests.Owned;

/// <summary>
/// PerMatchingLifetimeScope 测试
/// </summary>
[TestFixture]
public class PerMatchingLifetimeScopeTests
{
    /// <summary>
    /// 测试用例：PerMatchingLifetimeScope 注册
    /// </summary>
    [Test]
    public void Should_Register_PerMatchingLifetimeScope_Component()
    {
        // Arrange
        var container = ContainerFixture.CreateContainer();

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
        var container = ContainerFixture.CreateContainer();

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
        var container = ContainerFixture.CreateContainer();

        // Act & Assert - 两个标签都应该能解析
        using var scope1 = container.BeginLifetimeScope("tag1");
        var instance1 = scope1.Resolve<MultiTagScopeComponent>();
        Assert.That(instance1, Is.Not.Null);

        using var scope2 = container.BeginLifetimeScope("tag2");
        var instance2 = scope2.Resolve<MultiTagScopeComponent>();
        Assert.That(instance2, Is.Not.Null);
    }
}
