using Autofac;
using Csanno.Tests.Aop;
using NUnit.Framework;

namespace Csanno.Tests;

/// <summary>
/// AOP 拦截功能测试
/// </summary>
[TestFixture]
public class AopInterceptionTests
{
    [SetUp]
    public void SetUp()
    {
        LoggingInterceptor.ClearLogs();
        LoggingInterceptor2.ClearLogs();
        CacheInterceptor.ClearCache();
    }

    [Test]
    public void Interceptor_ShouldCallOnBeforeAndOnAfter()
    {
        // Arrange
        var builder = new ContainerBuilder();
        builder.RegisterComponents();
        builder.RegisterAopProxies();
        var container = builder.Build();

        // Act
        var service = container.Resolve<SampleService>();
        var result = service.Add(1, 2);

        // Assert
        Assert.That(result, Is.EqualTo(3));
        Assert.That(LoggingInterceptor.Logs, Does.Contain("[Before] Add(1, 2)"));
        Assert.That(LoggingInterceptor.Logs, Does.Contain("[After] Add => 3"));
    }

    [Test]
    public void Interceptor_ShouldSupportMultipleInterceptors()
    {
        // Arrange
        var builder = new ContainerBuilder();
        builder.RegisterComponents();
        builder.RegisterAopProxies();
        var container = builder.Build();

        // Act
        var service = container.Resolve<SampleService>();
        var result = service.Add(5, 3);

        // Assert
        Assert.That(result, Is.EqualTo(8));

        // 验证两个拦截器都被调用
        Assert.That(LoggingInterceptor.Logs, Does.Contain("[Before] Add(5, 3)"));
        Assert.That(LoggingInterceptor.Logs, Does.Contain("[After] Add => 8"));
        Assert.That(LoggingInterceptor2.Logs, Does.Contain("[Before-2] Add(5, 3)"));
        Assert.That(LoggingInterceptor2.Logs, Does.Contain("[After-2] Add => 8"));
    }

    [Test]
    public void Interceptor_ShouldWorkWithStringReturn()
    {
        // Arrange
        var builder = new ContainerBuilder();
        builder.RegisterComponents();
        builder.RegisterAopProxies();
        var container = builder.Build();

        // Act
        var service = container.Resolve<SampleService>();
        var result = service.Greet("World");

        // Assert
        Assert.That(result, Is.EqualTo("Hello, World!"));
        Assert.That(LoggingInterceptor.Logs, Does.Contain("[Before] Greet(World)"));
        Assert.That(LoggingInterceptor.Logs, Does.Contain("[After] Greet => Hello, World!"));
    }

    [Test]
    public void ProxyClass_ShouldBeResolvedInsteadOfOriginal()
    {
        // Arrange
        var builder = new ContainerBuilder();
        builder.RegisterComponents();
        builder.RegisterAopProxies();
        var container = builder.Build();

        // Act
        var service = container.Resolve<SampleService>();

        // Assert - 应该解析到代理类
        Assert.That(service.GetType().Name, Is.EqualTo("SampleService_Proxy"));
    }

    [Test]
    public void CacheInterceptor_ShouldTrackCacheHitsAndMisses()
    {
        // Arrange
        CacheInterceptor.ClearCache();
        var builder = new ContainerBuilder();
        builder.RegisterComponents();
        builder.RegisterAopProxies();
        var container = builder.Build();
        var service = container.Resolve<SampleService>();

        // Act - 第一次调用（缓存未命中）
        var result1 = service.ExpensiveCalculation(5);

        // Assert - 第一次调用
        Assert.That(result1, Is.EqualTo(25));
        Assert.That(CacheInterceptor.CacheMissCount, Is.EqualTo(1), "第一次调用应该是缓存未命中");
        Assert.That(CacheInterceptor.CacheHitCount, Is.EqualTo(0), "第一次调用不应该有缓存命中");

        // Act - 第二次调用（相同参数，缓存命中）
        var result2 = service.ExpensiveCalculation(5);

        // Assert - 第二次调用
        Assert.That(result2, Is.EqualTo(25));
        Assert.That(CacheInterceptor.CacheHitCount, Is.EqualTo(1), "第二次调用应该是缓存命中");
    }

    [Test]
    public void CacheInterceptor_ShouldMissOnDifferentParameters()
    {
        // Arrange
        CacheInterceptor.ClearCache();
        var builder = new ContainerBuilder();
        builder.RegisterComponents();
        builder.RegisterAopProxies();
        var container = builder.Build();
        var service = container.Resolve<SampleService>();

        // Act - 使用不同参数调用
        var result1 = service.ExpensiveCalculation(3);
        var result2 = service.ExpensiveCalculation(4);

        // Assert
        Assert.That(result1, Is.EqualTo(9));
        Assert.That(result2, Is.EqualTo(16));
        Assert.That(CacheInterceptor.CacheMissCount, Is.EqualTo(2), "不同参数应该都是缓存未命中");
        Assert.That(CacheInterceptor.CacheHitCount, Is.EqualTo(0), "不同参数不应该有缓存命中");
    }
}

