using Csanno.Attributes;

namespace Csanno.Tests.Registration.EdgeCases
{

    /// <summary>
    /// 抽象组件
    /// </summary>
    [Component]
    public abstract class AbstractComponent;

    /// <summary>
    /// 静态组件
    /// </summary>
    [Component]
    public static class StaticComponent
    {
        public static string Value => "static";
    }

    /// <summary>
    /// 私有构造函数组件
    /// </summary>
    [Component]
    public class PrivateConstructorComponent
    {
        private PrivateConstructorComponent() { }
    }

    /// <summary>
    /// 带缺失依赖的消费者
    /// </summary>
    [Component]
    public class ConsumerWithMissingDependency
    {
        public ConsumerWithMissingDependency(IMissingService service) { }
    }

    /// <summary>
    /// 缺失的服务接口
    /// </summary>
    public interface IMissingService { }

    /// <summary>
    /// 冲突生命周期组件
    /// </summary>
    [Component]
    [Transient]
    [Singleton]
    public class ConflictingLifetimeComponent;

    /// <summary>
    /// 默认生命周期组件
    /// </summary>
    [Component]
    public class DefaultLifetimeComponent;

    /// <summary>
    /// 非组件类
    /// </summary>
    public class NonComponent
    {
        public string Value => "not a component";
    }

    /// <summary>
    /// 带 Component 特性的基类
    /// </summary>
    [Component]
    public class BaseComponent;

    /// <summary>
    /// 继承 Component 特性的派生类
    /// </summary>
    public class DerivedComponent : BaseComponent;
}
