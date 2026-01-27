using System.Collections.Generic;

namespace Csanno.Generator.Models
{

    /// <summary>
    /// 代理类信息模型
    /// </summary>
    internal sealed class ProxyInfo
    {
        /// <summary>
        /// 程序集名称
        /// </summary>
        public string AssemblyName { get; set; } = string.Empty;

        /// <summary>
        /// 命名空间
        /// </summary>
        public string Namespace { get; set; } = string.Empty;

        /// <summary>
        /// 类名
        /// </summary>
        public string ClassName { get; set; } = string.Empty;

        /// <summary>
        /// 完整类型名称
        /// </summary>
        public string FullTypeName { get; set; } = string.Empty;

        /// <summary>
        /// 需要拦截的方法列表
        /// </summary>
        public List<MethodInterceptInfo> InterceptedMethods { get; set; } = new();

        /// <summary>
        /// 是否有需要拦截的方法
        /// </summary>
        public bool HasInterceptedMethods => InterceptedMethods.Count > 0;

        /// <summary>
        /// 构造函数列表
        /// </summary>
        public List<ProxyConstructorInfo> Constructors { get; set; } = new();
    }

    /// <summary>
    /// 需要拦截的方法信息
    /// </summary>
    internal sealed class MethodInterceptInfo
    {
        /// <summary>
        /// 方法名称
        /// </summary>
        public string MethodName { get; set; } = string.Empty;

        /// <summary>
        /// 返回类型
        /// </summary>
        public string ReturnType { get; set; } = string.Empty;

        /// <summary>
        /// 是否返回 void
        /// </summary>
        public bool ReturnsVoid { get; set; }

        /// <summary>
        /// 参数列表
        /// </summary>
        public List<ProxyParameterInfo> Parameters { get; set; } = new();

        /// <summary>
        /// 拦截器注解类型列表
        /// </summary>
        public List<string> InterceptorAttributeTypes { get; set; } = new();
    }

    /// <summary>
    /// 构造函数信息
    /// </summary>
    internal sealed class ProxyConstructorInfo
    {
        /// <summary>
        /// 参数列表
        /// </summary>
        public List<ProxyParameterInfo> Parameters { get; set; } = new();
    }

    /// <summary>
    /// 参数信息
    /// </summary>
    internal sealed class ProxyParameterInfo
    {
        /// <summary>
        /// 参数名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 参数类型
        /// </summary>
        public string Type { get; set; } = string.Empty;
    }
}
