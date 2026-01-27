using Csanno.Attributes;

namespace Csanno.Tests.Registration.Owned
{

    /// <summary>
    /// Owned 实例组件
    /// </summary>
    [Component]
    [Owned]
    public class OwnedComponent;

    /// <summary>
    /// 可释放的 Owned 实例组件
    /// </summary>
    [Component]
    [Owned]
    public class DisposableOwnedComponent : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }

    /// <summary>
    /// PerMatchingLifetimeScope 组件
    /// </summary>
    [Component]
    [PerMatchingLifetimeScope("request")]
    public class MatchingScopeComponent;

    /// <summary>
    /// 多标签 PerMatchingLifetimeScope 组件
    /// </summary>
    [Component]
    [PerMatchingLifetimeScope("tag1", "tag2")]
    public class MultiTagScopeComponent;
}
