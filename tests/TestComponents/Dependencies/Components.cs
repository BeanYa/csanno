using Csanno.Attributes;
using Csanno.Tests.TestComponents.Services;

namespace Csanno.Tests.TestComponents.Dependencies;

/// <summary>
/// 消费者组件，依赖 IService
/// </summary>
[Component]
public class Consumer
{
    public IService Service { get; }

    public Consumer(IService service)
    {
        Service = service;
    }
}

/// <summary>
/// 顶级消费者组件，依赖 Consumer
/// </summary>
[Component]
public class TopLevelConsumer
{
    public Consumer Consumer { get; }

    public TopLevelConsumer(Consumer consumer)
    {
        Consumer = consumer;
    }
}
