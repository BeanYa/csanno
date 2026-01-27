namespace Csanno.Attributes
{

    /// <summary>
    /// 指定组件应注册为的服务类型（通常用于接口映射）
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class AsServiceAttribute : Attribute
    {
        /// <summary>
        /// 获取服务类型
        /// </summary>
        public Type ServiceType { get; }

        /// <summary>
        /// 初始化 <see cref="AsServiceAttribute"/> 的新实例
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <exception cref="ArgumentNullException">当服务类型为 null 时抛出</exception>
        public AsServiceAttribute(Type serviceType)
        {
            ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
        }
    }
}
