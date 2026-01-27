using Csanno.Attributes;

namespace Csanno.Tests.Registration.Metadata
{

    /// <summary>
    /// 带元数据的组件接口
    /// </summary>
    public interface IComponentWithMetadata { }

    /// <summary>
    /// 带元数据的组件
    /// </summary>
    [Component]
    [AsService(typeof(IComponentWithMetadata))]
    [WithMetadata("Name", "TestComponent")]
    [WithMetadata("Version", 1)]
    public class ComponentWithMetadata : IComponentWithMetadata { }

    /// <summary>
    /// 带多种类型元数据的组件接口
    /// </summary>
    public interface IComponentWithMultipleMetadata { }

    /// <summary>
    /// 带多种类型元数据的组件
    /// </summary>
    [Component]
    [AsService(typeof(IComponentWithMultipleMetadata))]
    [WithMetadata("StringKey", "string value")]
    [WithMetadata("IntKey", 42)]
    [WithMetadata("BoolKey", true)]
    public class ComponentWithMultipleMetadataTypes : IComponentWithMultipleMetadata { }
}
