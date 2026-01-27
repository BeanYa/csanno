using Autofac;
using NUnit.Framework;

namespace Csanno.Tests.Aop
{

    /// <summary>
    /// InvokeResult 功能测试
    /// </summary>
    [TestFixture]
    public class InvokeResultTests
    {
        private IContainer _container = null!;

        [SetUp]
        public void SetUp()
        {
            var builder = new ContainerBuilder();
            builder.RegisterComponents();
            builder.RegisterAopProxies();
            _container = builder.Build();

            // 清理状态
            CacheInterceptor.ClearCache();
        }

        [TearDown]
        public void TearDown()
        {
            _container.Dispose();
        }

        /// <summary>
        /// 测试缓存拦截器通过 InvokeResult 返回缓存值
        /// </summary>
        [Test]
        public void CacheInterceptor_ShouldReturnCachedValue_ViaInvokeResult()
        {
            // Arrange
            var service = _container.Resolve<SampleService>();

            // Act - 第一次调用，生成时间戳
            var result1 = service.GetTimestamp();

            // Act - 第二次调用，应返回缓存的时间戳
            var result2 = service.GetTimestamp();

            // Assert
            Assert.That(result2, Is.EqualTo(result1), "第二次调用应返回缓存的时间戳");
            Assert.That(CacheInterceptor.CacheHitCount, Is.EqualTo(1), "应有一次缓存命中");
            Assert.That(CacheInterceptor.CacheMissCount, Is.EqualTo(1), "应有一次缓存未命中");
        }

        /// <summary>
        /// 测试缓存拦截器在多次调用时正确返回缓存值
        /// </summary>
        [Test]
        public void CacheInterceptor_MultipleCalls_ShouldReturnSameValue()
        {
            // Arrange
            var service = _container.Resolve<SampleService>();

            // Act - 多次调用
            var result1 = service.GetTimestamp();
            var result2 = service.GetTimestamp();
            var result3 = service.GetTimestamp();

            // Assert - 所有结果应相同
            Assert.That(result2, Is.EqualTo(result1));
            Assert.That(result3, Is.EqualTo(result1));
            Assert.That(CacheInterceptor.CacheHitCount, Is.EqualTo(2), "应有两次缓存命中");
            Assert.That(CacheInterceptor.CacheMissCount, Is.EqualTo(1), "应只有一次缓存未命中");
        }
    }
}
