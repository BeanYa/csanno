using Csanno.Attributes;

namespace Csanno.Tests;

/// <summary>
/// 服务接口
/// </summary>
public interface IService
{
    string DoWork();
}

/// <summary>
/// 实现服务接口的组件
/// </summary>
[Component]
[AsService(typeof(IService))]
public class ServiceWithInterface : IService
{
    public string DoWork() => "Work done";
}

/// <summary>
/// 服务接口1
/// </summary>
public interface IService1 { }

/// <summary>
/// 服务接口2
/// </summary>
public interface IService2 { }

/// <summary>
/// 实现多个服务接口的组件
/// </summary>
[Component]
[AsService(typeof(IService1))]
[AsService(typeof(IService2))]
public class MultiServiceComponent : IService1, IService2 { }
