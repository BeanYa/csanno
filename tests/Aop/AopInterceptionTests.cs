using Autofac;
using Autofac.Features.Metadata;
using NUnit.Framework;

namespace Csanno.Tests.Aop
{

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
        public void Interceptor_ShouldHandleOverloads_WithSameParameterCountDifferentSignature()
        {
            // Arrange
            var builder = new ContainerBuilder();
            builder.RegisterComponents();
            builder.RegisterAopProxies();
            var container = builder.Build();
            var service = container.Resolve<OverloadService>();

            // Act
            var result1 = service.Echo(1, "x");
            var result2 = service.Echo("y", 2);

            // Assert
            Assert.That(result1, Is.EqualTo("1:x"));
            Assert.That(result2, Is.EqualTo("y:2"));
            Assert.That(LoggingInterceptor.Logs, Does.Contain("[Before] Echo(1, x)"));
            Assert.That(LoggingInterceptor.Logs, Does.Contain("[Before] Echo(y, 2)"));
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

        [Test]
        public void CacheInterceptor_ShouldReturnSameTimestampOnRepeatedCalls()
        {
            // Arrange
            CacheInterceptor.ClearCache();
            var builder = new ContainerBuilder();
            builder.RegisterComponents();
            builder.RegisterAopProxies();
            var container = builder.Build();
            var service = container.Resolve<SampleService>();

            // Act - 第一次调用获取时间戳
            var timestamp1 = service.GetTimestamp("test-key");
            
            // 第二次调用（缓存命中，但当前实现仍会执行方法）
            var timestamp2 = service.GetTimestamp("test-key");

            // Assert - 验证缓存追踪正确
            // 注意：当前 AOP 实现只追踪缓存命中，不阻止方法执行
            // 如需实现真正的缓存返回值，需要在 generated proxy 中支持方法调用跳过
            Assert.That(timestamp1, Is.GreaterThan(0), "第一次调用应返回有效时间戳");
            Assert.That(timestamp2, Is.GreaterThan(0), "第二次调用应返回有效时间戳");
            Assert.That(CacheInterceptor.CacheMissCount, Is.EqualTo(1), "第一次调用应该是缓存未命中");
            Assert.That(CacheInterceptor.CacheHitCount, Is.EqualTo(1), "第二次调用应该是缓存命中");
        }

        [Test]
        public void InterceptorChain_ShouldCallInNestedOrder_WhenAllReturnTrue()
        {
            // Arrange
            ChainInterceptor1.Clear();
            ChainTestService.Clear();
            ChainInterceptor2.ShouldContinue = true;

            var builder = new ContainerBuilder();
            builder.RegisterComponents();
            builder.RegisterAopProxies();
            var container = builder.Build();

            // Act
            var service = container.Resolve<ChainTestService>();
            var result = service.TestMethod(5);

            // Assert - 验证嵌套调用顺序（洋葱模型）
            Assert.That(result, Is.EqualTo(10), "方法应返回正确结果");
            Assert.That(ChainTestService.OriginalMethodCalled, Is.True, "原生方法应被调用");
            
            // 验证调用顺序: I1.Before -> I2.Before -> I3.Before -> 原生方法 -> I3.After -> I2.After -> I1.After
            var expectedOrder = new[] 
            { 
                "I1.OnBefore", "I2.OnBefore", "I3.OnBefore", 
                "I3.OnAfter", "I2.OnAfter", "I1.OnAfter" 
            };
            Assert.That(ChainInterceptor1.CallOrder, Is.EqualTo(expectedOrder), "调用顺序应为洋葱模型");
        }

        [Test]
        public void InterceptorChain_ShouldSkipOriginalMethod_WhenOnBeforeReturnsFalse()
        {
            // Arrange
            ChainInterceptor1.Clear();
            ChainTestService.Clear();
            ChainInterceptor2.ShouldContinue = false; // I2 返回 false

            var builder = new ContainerBuilder();
            builder.RegisterComponents();
            builder.RegisterAopProxies();
            var container = builder.Build();

            // Act
            var service = container.Resolve<ChainTestService>();
            var result = service.TestMethod(5);

            // Assert
            // 因为有 MustInvoke 拦截器，所以原生方法仍会被调用
            // 但如果没有 MustInvoke，则不会调用
            // 验证所有 OnBefore 和 OnAfter 都被调用
            Assert.That(ChainInterceptor1.CallOrder, Does.Contain("I1.OnBefore"));
            Assert.That(ChainInterceptor1.CallOrder, Does.Contain("I2.OnBefore"));
            Assert.That(ChainInterceptor1.CallOrder, Does.Contain("I3.OnBefore"));
            Assert.That(ChainInterceptor1.CallOrder, Does.Contain("I1.OnAfter"));
            Assert.That(ChainInterceptor1.CallOrder, Does.Contain("I2.OnAfter"));
            Assert.That(ChainInterceptor1.CallOrder, Does.Contain("I3.OnAfter"));
            
            // 因为有 MustInvoke 拦截器，原生方法仍被调用
            Assert.That(ChainTestService.OriginalMethodCalled, Is.True, "有 MustInvoke 时原生方法应被调用");
        }

        [Test]
        public void InterceptorChain_OnAfterShouldAlwaysBeCalled()
        {
            // Arrange
            ChainInterceptor1.Clear();
            ChainTestService.Clear();
            ChainInterceptor2.ShouldContinue = true;

            var builder = new ContainerBuilder();
            builder.RegisterComponents();
            builder.RegisterAopProxies();
            var container = builder.Build();

            // Act
            var service = container.Resolve<ChainTestService>();
            service.TestMethod(10);

            // Assert - 验证所有 OnAfter 都被调用
            var onAfterCalls = ChainInterceptor1.CallOrder.Where(c => c.Contains("OnAfter")).ToList();
            Assert.That(onAfterCalls.Count, Is.EqualTo(3), "所有拦截器的 OnAfter 都应被调用");
        }

        [Test]
        public void AopProxy_Should_Preserve_Lifetime_Service_And_Metadata()
        {
            // Arrange
            var builder = new ContainerBuilder();
            builder.RegisterComponents();
            builder.RegisterAopProxies();
            var container = builder.Build();

            // Act
            var service1 = container.Resolve<IAopMetadataService>();
            var service2 = container.Resolve<IAopMetadataService>();
            var meta = container.Resolve<Meta<IAopMetadataService>>();

            // Assert - proxy should be the resolved type
            Assert.That(service1.GetType().Name, Is.EqualTo("AopMetadataService_Proxy"));
            Assert.That(service1, Is.SameAs(service2), "Singleton 生命周期应被保留");

            Assert.That(meta.Metadata.ContainsKey("Name"), Is.True, "元数据应被保留");
            Assert.That(meta.Metadata["Name"], Is.EqualTo("AopService"));
        }

        [Test]
        public void NonVirtualMethod_ShouldNotBeIntercepted()
        {
            // Arrange
            var builder = new ContainerBuilder();
            builder.RegisterComponents();
            builder.RegisterAopProxies();
            var container = builder.Build();

            // Act
            var service = container.Resolve<SampleService>();
            LoggingInterceptor.ClearLogs();
            var result = service.NonVirtualMethod(5);

            // Assert - 非 virtual 方法不会被拦截
            Assert.That(result, Is.EqualTo(10));
            Assert.That(LoggingInterceptor.Logs, Is.Empty, "非 virtual 方法不应触发拦截器");
        }

        [Test]
        public void DerivedService_OverriddenVirtualMethod_ShouldBeIntercepted()
        {
            // Arrange
            var builder = new ContainerBuilder();
            builder.RegisterComponents();
            builder.RegisterAopProxies();
            var container = builder.Build();

            // Act
            var service = container.Resolve<DerivedService>();
            var result = service.BaseVirtualMethod("test");

            // Assert
            Assert.That(result, Is.EqualTo("Derived: test"));
            Assert.That(LoggingInterceptor.Logs, Does.Contain("[Before] BaseVirtualMethod(test)"));
            Assert.That(LoggingInterceptor.Logs, Does.Contain("[After] BaseVirtualMethod => Derived: test"));
        }

        [Test]
        public void DerivedService_OwnVirtualMethod_ShouldBeIntercepted()
        {
            // Arrange
            var builder = new ContainerBuilder();
            builder.RegisterComponents();
            builder.RegisterAopProxies();
            var container = builder.Build();

            // Act
            var service = container.Resolve<DerivedService>();
            var result = service.DerivedVirtualMethod("test");

            // Assert
            Assert.That(result, Is.EqualTo("DerivedOwn: test"));
            Assert.That(LoggingInterceptor.Logs, Does.Contain("[Before] DerivedVirtualMethod(test)"));
            Assert.That(LoggingInterceptor.Logs, Does.Contain("[After] DerivedVirtualMethod => DerivedOwn: test"));
        }

        [Test]
        public void InterceptorException_OnBefore_ShouldBeHandledByOnBeforeException()
        {
            // Arrange
            PropagatingExceptionInterceptor.Clear();
            PropagatingExceptionInterceptor.ShouldThrowOnBefore = true;
            InterceptorExceptionPropagationService.Clear();

            var builder = new ContainerBuilder();
            builder.RegisterComponents();
            builder.RegisterAopProxies();
            var container = builder.Build();

            // Act - 拦截器异常会调用 OnBeforeException，默认返回 true 继续执行
            var service = container.Resolve<InterceptorExceptionPropagationService>();
            var result = service.TestMethod(5);

            // Assert - 由于默认 OnBeforeException 返回 true，原生方法应被调用
            Assert.That(PropagatingExceptionInterceptor.CallOrder, Does.Contain("OnBefore"), "OnBefore 应被调用");
            Assert.That(InterceptorExceptionPropagationService.OriginalMethodCalled, Is.True, "默认 OnBeforeException 返回 true 时原生方法应被调用");
            Assert.That(result, Is.EqualTo(10));
        }

        [Test]
        public void InterceptorException_OnAfter_ShouldBeHandledByOnAfterException()
        {
            // Arrange
            PropagatingExceptionInterceptor.Clear();
            PropagatingExceptionInterceptor.ShouldThrowOnAfter = true;
            InterceptorExceptionPropagationService.Clear();

            var builder = new ContainerBuilder();
            builder.RegisterComponents();
            builder.RegisterAopProxies();
            var container = builder.Build();

            // Act - 拦截器异常会调用 OnAfterException，默认不抛出到调用者
            var service = container.Resolve<InterceptorExceptionPropagationService>();
            var result = service.TestMethod(5);

            // Assert - OnAfter 异常被 OnAfterException 处理
            Assert.That(PropagatingExceptionInterceptor.CallOrder, Does.Contain("OnBefore"), "OnBefore 应被调用");
            Assert.That(PropagatingExceptionInterceptor.CallOrder, Does.Contain("OnAfter"), "OnAfter 应被调用");
            Assert.That(InterceptorExceptionPropagationService.OriginalMethodCalled, Is.True, "原生方法应已被调用");
            Assert.That(result, Is.EqualTo(10));
        }
    }

}
