using NUnit.Framework;
using Autofac;
using Csanno.Tests.Registration.Services;

namespace Csanno.Tests;

/// <summary>
/// 服务接口映射测试
/// </summary>
[TestFixture]
public class ServiceRegistrationTests
{
    /// <summary>
    /// 测试用例：服务接口映射
    /// </summary>
    [Test]
    public void Should_Register_Component_As_Interface()
    {
        // Arrange
        var container = ContainerFixture.CreateContainer();

        // Act
        var service = container.Resolve<IService>();

        // Assert
        Assert.That(service, Is.InstanceOf<ServiceWithInterface>());
        Assert.That(service.DoWork(), Is.EqualTo("Work done"));
    }

    /// <summary>
    /// 测试用例：多个服务接口注册
    /// </summary>
    [Test]
    public void Should_Register_Component_As_Multiple_Services()
    {
        // Arrange
        var container = ContainerFixture.CreateContainer();

        // Act
        var service1 = container.Resolve<IService1>();
        var service2 = container.Resolve<IService2>();

        // Assert
        Assert.That(service1, Is.InstanceOf<MultiServiceComponent>());
        Assert.That(service2, Is.InstanceOf<MultiServiceComponent>());
        // 注意：由于默认是 Transient 生命周期，解析不同的服务接口会创建新实例
        // 如果需要同一实例，需要使用 Scoped 或 Singleton
    }
}
