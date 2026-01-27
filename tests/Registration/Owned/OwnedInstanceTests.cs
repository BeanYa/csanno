using NUnit.Framework;
using Autofac;
using Autofac.Features.OwnedInstances;
using Csanno.Tests.Registration.Owned;

namespace Csanno.Tests
{

    /// <summary>
    /// Owned 实例测试
    /// </summary>
    [TestFixture]
    public class OwnedInstanceTests
    {
        /// <summary>
        /// 测试用例：Owned 实例注册和解析
        /// </summary>
        [Test]
        public void Should_Register_Owned_Component()
        {
            // Arrange
            var container = ContainerFixture.CreateContainer();

            // Act
            using var scope = container.BeginLifetimeScope();
            var owned = scope.Resolve<Owned<OwnedComponent>>();

            // Assert
            Assert.That(owned, Is.Not.Null);
            Assert.That(owned.Value, Is.Not.Null);
        }

        /// <summary>
        /// 测试用例：Owned 实例释放后不应再使用
        /// </summary>
        [Test]
        public void Owned_Component_Should_Be_Disposed()
        {
            // Arrange
            var container = ContainerFixture.CreateContainer();

            // Act
            DisposableOwnedComponent? instance = null;
            using (var owned = container.Resolve<Owned<DisposableOwnedComponent>>())
            {
                instance = owned.Value;
            }

            // Assert
            Assert.That(instance, Is.Not.Null);
            Assert.That(instance.IsDisposed, Is.True);
        }
    }
}
