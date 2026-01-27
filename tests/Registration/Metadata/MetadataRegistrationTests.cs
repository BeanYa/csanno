using NUnit.Framework;
using Autofac;
using Autofac.Features.Metadata;
using Csanno.Tests.Registration.Metadata;

namespace Csanno.Tests
{

    /// <summary>
    /// 元数据注册测试
    /// </summary>
    [TestFixture]
    public class MetadataRegistrationTests
    {
        /// <summary>
        /// 测试用例：元数据注册和检索
        /// </summary>
        [Test]
        public void Should_Register_Component_With_Metadata()
        {
            // Arrange
            var container = ContainerFixture.CreateContainer();

            // Act - 使用 Meta<T> 强类型元数据来解析
            var meta = container.Resolve<Meta<IComponentWithMetadata>>();

            // Assert
            Assert.That(meta, Is.Not.Null);
            Assert.That(meta.Metadata.TryGetValue("Name", out var name), Is.True);
            Assert.That(name, Is.EqualTo("TestComponent"));
            Assert.That(meta.Metadata.TryGetValue("Version", out var version), Is.True);
            Assert.That(version, Is.EqualTo(1));
        }

        /// <summary>
        /// 测试用例：元数据支持多种值类型
        /// </summary>
        [Test]
        public void Should_Register_Component_With_Multiple_Metadata_Types()
        {
            // Arrange
            var container = ContainerFixture.CreateContainer();

            // Act
            var meta = container.Resolve<Meta<IComponentWithMultipleMetadata>>();

            // Assert
            Assert.That(meta.Metadata["StringKey"], Is.EqualTo("string value"));
            Assert.That(meta.Metadata["IntKey"], Is.EqualTo(42));
            Assert.That(meta.Metadata["BoolKey"], Is.EqualTo(true));
        }
    }
}
