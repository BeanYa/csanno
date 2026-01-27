using System.Collections.Generic;

namespace Csanno.Generator.Models
{

    /// <summary>
    /// 拦截器信息模型
    /// </summary>
    internal sealed class InterceptorInfo
    {
        /// <summary>
        /// 完整类型名称
        /// </summary>
        public string FullTypeName { get; set; } = string.Empty;

        /// <summary>
        /// 类名
        /// </summary>
        public string ClassName { get; set; } = string.Empty;

        /// <summary>
        /// 拦截器绑定列表
        /// </summary>
        public List<InterceptorBinding> Bindings { get; set; } = new();

        /// <summary>
        /// 向后兼容：获取绑定的注解类型列表
        /// </summary>
        public List<string> BoundAttributeTypes => Bindings.ConvertAll(b => b.AttributeType);
    }

    /// <summary>
    /// 拦截器绑定信息
    /// </summary>
    internal sealed class InterceptorBinding
    {
        /// <summary>
        /// 绑定的注解类型
        /// </summary>
        public string AttributeType { get; set; } = string.Empty;

        /// <summary>
        /// 原生方法调用类型
        /// </summary>
        public string InvokeType { get; set; } = "Default";
    }
}
