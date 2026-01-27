using Csanno.Attributes;
using Csanno.Tests.Registration.Services;

namespace Csanno.Tests.Registration.Dependencies
{

    /// <summary>
    /// 消费者组件，依赖 IService
    /// </summary>
    [Component]
    public class Consumer(IService service)
    {
        public IService Service { get; } = service;
    }

    /// <summary>
    /// 顶级消费者组件，依赖 Consumer
    /// </summary>
    [Component]
    public class TopLevelConsumer(Consumer consumer)
    {
        public Consumer Consumer { get; } = consumer;
    }
}
