namespace Csanno.Attributes
{

    /// <summary>
    /// 为组件注册添加元数据
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class WithMetadataAttribute : Attribute
    {
        /// <summary>
        /// 初始化元数据条目
        /// </summary>
        /// <param name="key">元数据键</param>
        /// <param name="value">元数据值</param>
        public WithMetadataAttribute(string key, object? value)
        {
            Key = key;
            Value = value;
        }

        /// <summary>
        /// 获取元数据键
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// 获取元数据值
        /// </summary>
        public object? Value { get; }
    }
}
