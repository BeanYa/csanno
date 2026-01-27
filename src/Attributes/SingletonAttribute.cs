namespace Csanno.Attributes
{

    /// <summary>
    /// 标记组件为 Singleton 生命周期（容器内全局单例）
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class SingletonAttribute : Attribute
    {
    }
}
