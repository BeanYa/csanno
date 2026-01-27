namespace Csanno.Internal
{

    /// <summary>
    /// 组件实例生命周期类型
    /// </summary>
    internal enum InstanceLifetime
    {
        /// <summary>
        /// 每次解析创建新实例
        /// </summary>
        Transient,

        /// <summary>
        /// 同一生命周期作用域内返回同一实例
        /// </summary>
        Scoped,

        /// <summary>
        /// 容器内全局单例
        /// </summary>
        Singleton,

        /// <summary>
        /// 每个 HTTP 请求内返回同一实例
        /// </summary>
        PerRequest,

        /// <summary>
        /// 在匹配指定标签的生命周期作用域内返回同一实例
        /// </summary>
        PerMatchingLifetimeScope,

        /// <summary>
        /// 作为拥有实例注册
        /// </summary>
        Owned,
    }
}
