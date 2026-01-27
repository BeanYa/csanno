namespace Csanno.Attributes
{

    /// <summary>
    /// 标记组件为 Transient 生命周期（每次解析创建新实例）
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class TransientAttribute : Attribute
    {
    }
}
