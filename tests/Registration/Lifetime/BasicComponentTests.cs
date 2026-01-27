using NUnit.Framework;
using Autofac;
using Csanno.Tests.Registration.Lifetime;

namespace Csanno.Tests
{

    /// <summary>
    /// 基础组件注册测试
    /// </summary>
    [TestFixture]
    public class BasicComponentTests
    {
        /// <summary>
        /// 测试用例：基础组件注册和解析
        /// </summary>
        [Test]
        public void Should_Register_And_Resolve_Basic_Component()
        {
            // Arrange
            var builder = ContainerFixture.CreateBuilder();

            // Act
            var container = builder.Build();
            var component = container.Resolve<SimpleComponent>();

            // Assert
            Assert.That(component, Is.Not.Null);
            Assert.That(component.Greet(), Is.EqualTo("Hello from SimpleComponent"));
        }
    }
}
