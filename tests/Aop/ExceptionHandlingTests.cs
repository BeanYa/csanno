using Autofac;
using NUnit.Framework;

namespace Csanno.Tests.Aop
{

    /// <summary>
    /// 拦截器异常处理测试
    /// </summary>
    [TestFixture]
    public class ExceptionHandlingTests
    {
        [SetUp]
        public void SetUp()
        {
            ThrowingOnBeforeInterceptor.Clear();
            ThrowingOnAfterInterceptor.Clear();
            ExceptionTestService.Clear();
        }

        [Test]
        public void OnBeforeException_ShouldBeCalled_WhenOnBeforeThrows()
        {
            // Arrange
            ThrowingOnBeforeInterceptor.ShouldThrow = true;
            ThrowingOnBeforeInterceptor.ShouldContinueOnException = true;

            var builder = new ContainerBuilder();
            builder.RegisterComponents();
            builder.RegisterAopProxies();
            var container = builder.Build();

            // Act
            var service = container.Resolve<ExceptionTestService>();
            var result = service.TestMethod(5);

            // Assert
            Assert.That(ThrowingOnBeforeInterceptor.CallOrder, Does.Contain("OnBefore"));
            Assert.That(ThrowingOnBeforeInterceptor.CallOrder, Does.Contain("OnBeforeException"));
            Assert.That(ThrowingOnBeforeInterceptor.CaughtExceptions.Count, Is.GreaterThan(0));
            Assert.That(ThrowingOnBeforeInterceptor.CaughtExceptions[0].Message, Is.EqualTo("OnBefore exception"));
        }

        [Test]
        public void OnBeforeException_ShouldContinueChain_WhenReturnsTrue()
        {
            // Arrange
            ThrowingOnBeforeInterceptor.ShouldThrow = true;
            ThrowingOnBeforeInterceptor.ShouldContinueOnException = true;

            var builder = new ContainerBuilder();
            builder.RegisterComponents();
            builder.RegisterAopProxies();
            var container = builder.Build();

            // Act
            var service = container.Resolve<ExceptionTestService>();
            var result = service.TestMethod(5);

            // Assert - 原生方法应该被调用
            Assert.That(ExceptionTestService.OriginalMethodCalled, Is.True);
            Assert.That(result, Is.EqualTo(10));
        }

        [Test]
        public void OnBeforeException_ShouldStopChain_WhenReturnsFalse()
        {
            // Arrange
            ThrowingOnBeforeInterceptor.ShouldThrow = true;
            ThrowingOnBeforeInterceptor.ShouldContinueOnException = false;

            var builder = new ContainerBuilder();
            builder.RegisterComponents();
            builder.RegisterAopProxies();
            var container = builder.Build();

            // Act
            var service = container.Resolve<ExceptionTestService>();
            var result = service.TestMethod(5);

            // Assert - 原生方法不应该被调用
            Assert.That(ExceptionTestService.OriginalMethodCalled, Is.False);
            Assert.That(result, Is.EqualTo(default(int)));
        }

        [Test]
        public void OnAfterException_ShouldBeCalled_WhenOnAfterThrows()
        {
            // Arrange
            ThrowingOnAfterInterceptor.ShouldThrow = true;

            var builder = new ContainerBuilder();
            builder.RegisterComponents();
            builder.RegisterAopProxies();
            var container = builder.Build();

            // Act
            var service = container.Resolve<ExceptionTestService>();
            service.TestMethod(5);

            // Assert
            Assert.That(ThrowingOnAfterInterceptor.CallOrder, Does.Contain("OnAfter"));
            Assert.That(ThrowingOnAfterInterceptor.CallOrder, Does.Contain("OnAfterException"));
            Assert.That(ThrowingOnAfterInterceptor.CaughtExceptions.Count, Is.GreaterThan(0));
            Assert.That(ThrowingOnAfterInterceptor.CaughtExceptions[0].Message, Is.EqualTo("OnAfter exception"));
        }

        [Test]
        public void OnAfter_ShouldStillBeCalled_WhenOnBeforeExceptionHandled()
        {
            // Arrange
            ThrowingOnBeforeInterceptor.ShouldThrow = true;
            ThrowingOnBeforeInterceptor.ShouldContinueOnException = true;

            var builder = new ContainerBuilder();
            builder.RegisterComponents();
            builder.RegisterAopProxies();
            var container = builder.Build();

            // Act
            var service = container.Resolve<ExceptionTestService>();
            service.TestMethod(5);

            // Assert - OnAfter 应该被调用
            Assert.That(ThrowingOnBeforeInterceptor.CallOrder, Does.Contain("OnAfter"));
        }

        [Test]
        public void VoidMethod_ShouldHandleOnBeforeException()
        {
            // Arrange
            ThrowingOnBeforeInterceptor.ShouldThrow = true;
            ThrowingOnBeforeInterceptor.ShouldContinueOnException = true;

            var builder = new ContainerBuilder();
            builder.RegisterComponents();
            builder.RegisterAopProxies();
            var container = builder.Build();

            // Act
            var service = container.Resolve<ExceptionTestService>();
            service.VoidMethod();

            // Assert
            Assert.That(ThrowingOnBeforeInterceptor.CallOrder, Does.Contain("OnBeforeException"));
            Assert.That(ExceptionTestService.OriginalMethodCalled, Is.True);
        }
    }
}
