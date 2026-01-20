using System.Reflection;
using Autofac;
using Autofac.Builder;
using Csanno.Internal;

namespace Csanno;

/// <summary>
/// Autofac 容器构建器扩展方法，用于注册带特性的组件
/// </summary>
public static class RegistrationExtensions
{
    /// <summary>
    /// 注册指定程序集中的所有带 [Component] 特性的组件
    /// </summary>
    /// <param name="builder">容器构建器</param>
    /// <param name="assemblies">要扫描的程序集</param>
    /// <returns>容器构建器，用于链式调用</returns>
    public static ContainerBuilder RegisterComponents(
        this ContainerBuilder builder,
        params Assembly[] assemblies)
    {
        var registrations = ComponentScanner.Scan(assemblies);

        foreach (var registration in registrations)
        {
            RegisterComponent(builder, registration);
        }

        return builder;
    }

    /// <summary>
    /// 注册调用程序集中的所有带 [Component] 特性的组件
    /// </summary>
    /// <param name="builder">容器构建器</param>
    /// <returns>容器构建器，用于链式调用</returns>
    public static ContainerBuilder RegisterComponents(this ContainerBuilder builder)
    {
        var callingAssembly = Assembly.GetCallingAssembly();
        return builder.RegisterComponents(callingAssembly);
    }

    /// <summary>
    /// 注册指定类型所在程序集中的所有带 [Component] 特性的组件
    /// </summary>
    /// <typeparam name="T">用于定位程序集的类型</typeparam>
    /// <param name="builder">容器构建器</param>
    /// <returns>容器构建器，用于链式调用</returns>
    public static ContainerBuilder RegisterComponentsFromType<T>(this ContainerBuilder builder)
    {
        var assembly = typeof(T).Assembly;
        return builder.RegisterComponents(assembly);
    }

    /// <summary>
    /// 注册单个组件
    /// </summary>
    private static void RegisterComponent(ContainerBuilder builder, ComponentRegistration registration)
    {
        var registrationBuilder = builder.RegisterType(registration.ComponentType);

        // 应用生命周期
        switch (registration.Lifetime)
        {
            case InstanceLifetime.Transient:
                registrationBuilder.InstancePerDependency();
                break;
            case InstanceLifetime.Scoped:
                registrationBuilder.InstancePerLifetimeScope();
                break;
            case InstanceLifetime.Singleton:
                registrationBuilder.SingleInstance();
                break;
            case InstanceLifetime.PerRequest:
                registrationBuilder.InstancePerRequest();
                break;
            case InstanceLifetime.PerMatchingLifetimeScope:
                registrationBuilder.InstancePerMatchingLifetimeScope(registration.LifetimeScopeTags ?? []);
                break;
            case InstanceLifetime.Owned:
                if (registration.OwnedType is not null)
                {
                    registrationBuilder.InstancePerOwned(registration.OwnedType);
                }
                else
                {
                    registrationBuilder.InstancePerOwned(registration.ComponentType);
                }
                break;
        }

        // 应用服务类型映射
        foreach (var serviceType in registration.ServiceTypes)
        {
            registrationBuilder.As(serviceType);
        }

        // 应用元数据
        if (registration.Metadata is not null && registration.Metadata.Count > 0)
        {
            registrationBuilder.WithMetadata(registration.Metadata);
        }
    }
}
