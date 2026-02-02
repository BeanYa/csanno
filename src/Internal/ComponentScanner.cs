using System.Reflection;
using Csanno.Attributes;

namespace Csanno.Internal
{
    /// <summary>
    /// 反射类型加载异常事件参数
    /// </summary>
    public class ReflectionTypeLoadExceptionEventArgs : EventArgs
    {
        /// <summary>
        /// 发生异常的程序集
        /// </summary>
        public Assembly Assembly { get; }

        /// <summary>
        /// 加载失败的异常列表
        /// </summary>
        public Exception?[] LoaderExceptions { get; }

        public ReflectionTypeLoadExceptionEventArgs(Assembly assembly, Exception?[] loaderExceptions)
        {
            Assembly = assembly;
            LoaderExceptions = loaderExceptions;
        }
    }

    /// <summary>
    /// 组件扫描器，用于扫描程序集并识别带特性的组件
    /// </summary>
    internal static class ComponentScanner
    {
        /// <summary>
        /// 当扫描程序集时发生 ReflectionTypeLoadException 时触发，暴露 LoaderExceptions 信息
        /// </summary>
        public static event EventHandler<ReflectionTypeLoadExceptionEventArgs>? TypeLoadExceptionOccurred;

        /// <summary>
        /// 扫描指定的程序集，返回所有带 [Component] 特性的组件注册信息
        /// </summary>
        /// <param name="assemblies">要扫描的程序集</param>
        /// <returns>组件注册信息集合</returns>
        public static IEnumerable<ComponentRegistration> Scan(IEnumerable<Assembly> assemblies)
        {
            foreach (var assembly in assemblies)
            {
                foreach (var registration in ScanAssembly(assembly))
                {
                    yield return registration;
                }
            }
        }

        /// <summary>
        /// 扫描单个程序集
        /// </summary>
        private static IEnumerable<ComponentRegistration> ScanAssembly(Assembly assembly)
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                // 触发事件，暴露 LoaderExceptions 信息给调用者
                TypeLoadExceptionOccurred?.Invoke(null, new ReflectionTypeLoadExceptionEventArgs(
                    assembly,
                    ex.LoaderExceptions ?? []
                ));
                types = ex.Types.Where(t => t is not null).Cast<Type>().ToArray();
            }

            foreach (var type in types)
            {
                if (TryGetComponentRegistration(type, out var registration) && registration is not null)
                {
                    yield return registration;
                }
            }
        }

        /// <summary>
        /// 尝试从类型获取组件注册信息
        /// </summary>
        private static bool TryGetComponentRegistration(Type type, out ComponentRegistration? registration)
        {
            registration = null;

            // 使用 IsDefined 做快速过滤，避免不必要的 GetCustomAttributes 分配
            if (!type.IsDefined(typeof(ComponentAttribute), inherit: true))
            {
                return false;
            }

            // 检查是否有 Component 特性
            var componentAttrs = type.GetCustomAttributes<ComponentAttribute>(inherit: true).ToArray();

            // 过滤无效类型
            if (!IsValidComponentType(type))
            {
                return false;
            }

            // 解析生命周期
            var lifetime = ResolveLifetime(type, out var lifetimeScopeTags, out var ownedType);

            // 解析服务类型
            var serviceTypes = ResolveServiceTypes(type, componentAttrs);

            // 解析元数据
            var metadata = ResolveMetadata(type);

            registration = new ComponentRegistration(
                ComponentType: type,
                Lifetime: lifetime,
                ServiceTypes: serviceTypes,
                Metadata: metadata,
                LifetimeScopeTags: lifetimeScopeTags,
                OwnedType: ownedType
            );

            return true;
        }

        /// <summary>
        /// 检查类型是否为有效的组件类型
        /// </summary>
        private static bool IsValidComponentType(Type type)
        {
            // 排除接口
            if (type.IsInterface)
            {
                return false;
            }

            // 排除静态类 (静态类是抽象且密封的)
            if (type.IsClass && type.IsAbstract && type.IsSealed)
            {
                return false;
            }

            // 排除抽象类（但要在静态类检查之后）
            if (type.IsAbstract)
            {
                return false;
            }

            // 排除值类型
            if (type.IsValueType)
            {
                return false;
            }

            // 检查是否有公共构造函数
            var hasPublicConstructor = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance).Length > 0;
            if (!hasPublicConstructor)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 解析生命周期，按优先级查找特性
        /// </summary>
        private static InstanceLifetime ResolveLifetime(
            Type type,
            out string[]? lifetimeScopeTags,
            out Type? ownedType)
        {
            lifetimeScopeTags = null;
            ownedType = null;

            // 按优先级检查（从高到低）
            // 1. Singleton
            if (type.GetCustomAttribute<SingletonAttribute>() is not null)
            {
                return InstanceLifetime.Singleton;
            }

            // 2. PerMatchingLifetimeScope
            var perMatchingAttr = type.GetCustomAttribute<PerMatchingLifetimeScopeAttribute>();
            if (perMatchingAttr is not null)
            {
                lifetimeScopeTags = perMatchingAttr.Tags;
                return InstanceLifetime.PerMatchingLifetimeScope;
            }

            // 3. Scoped
            if (type.GetCustomAttribute<ScopedAttribute>() is not null)
            {
                return InstanceLifetime.Scoped;
            }

            // 4. PerRequest
            if (type.GetCustomAttribute<PerRequestAttribute>() is not null)
            {
                return InstanceLifetime.PerRequest;
            }

            // 5. Owned
            var ownedAttr = type.GetCustomAttribute<OwnedAttribute>();
            if (ownedAttr is not null)
            {
                ownedType = ownedAttr.OwnedType;
                return InstanceLifetime.Owned;
            }

            // 6. Transient (默认)
            return InstanceLifetime.Transient;
        }

        /// <summary>
        /// 解析服务类型
        /// </summary>
        private static Type[] ResolveServiceTypes(Type type, ComponentAttribute[] componentAttrs)
        {
            var serviceTypes = new List<Type>();

            // 1. 收集所有 [AsService] 特性指定的类型
            var asServiceAttrs = type.GetCustomAttributes<AsServiceAttribute>();
            foreach (var attr in asServiceAttrs)
            {
                serviceTypes.Add(attr.ServiceType);
            }

            // 2. 如果有 [AsService]，使用指定的类型
            if (serviceTypes.Count > 0)
            {
                return [.. serviceTypes];
            }

            // 3. 如果 ComponentAttribute.ServiceType 有值，使用这些类型
            var componentServiceTypes = componentAttrs
                .Select(attr => attr.ServiceType)
                .Where(t => t is not null)
                .Cast<Type>()
                .Distinct()
                .ToList();
            if (componentServiceTypes.Count > 0)
            {
                return [.. componentServiceTypes];
            }

            // 4. 默认使用类本身
            return [type];
        }

        /// <summary>
        /// 解析元数据
        /// </summary>
        private static IDictionary<string, object?>? ResolveMetadata(Type type)
        {
            var metadataAttrs = type.GetCustomAttributes<WithMetadataAttribute>();
            if (!metadataAttrs.Any())
            {
                return null;
            }

            var metadata = new Dictionary<string, object?>();
            foreach (var attr in metadataAttrs)
            {
                metadata[attr.Key] = attr.Value;
            }

            return metadata;
        }
    }
}
