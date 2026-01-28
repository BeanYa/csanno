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

    /// <summary>
    /// 扩展元数据组件接口
    /// </summary>
    public interface IComponentWithExtendedMetadata { }

    /// <summary>
    /// 元数据枚举
    /// </summary>
    public enum MetadataEnum
    {
        Zero = 0,
        One = 1
    }

    /// <summary>
    /// 带扩展元数据类型的组件
    /// </summary>
    [Component]
    [AsService(typeof(IComponentWithExtendedMetadata))]
    [WithMetadata("Quote", "a\"b\\c")]
    [WithMetadata("LongKey", 123L)]
    [WithMetadata("DoubleKey", 1.5)]
    [WithMetadata("CharKey", 'x')]
    [WithMetadata("EnumKey", MetadataEnum.One)]
    [WithMetadata("TypeKey", typeof(ComponentWithMetadata))]
    public class ComponentWithExtendedMetadata : IComponentWithExtendedMetadata { }
}
