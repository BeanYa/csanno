namespace Csanno.Attributes
{

    /// <summary>
    /// 拦截器注解基类，所有用于方法拦截的注解都应继承此类
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public abstract class BaseInterceptAttribute : Attribute
    {
    }
}
