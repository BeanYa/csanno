namespace Csanno.Attributes
{

    /// <summary>
    /// 标记组件为 PerRequest 生命周期（每个 HTTP 请求内返回同一实例）
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class PerRequestAttribute : Attribute
    {
    }
}
