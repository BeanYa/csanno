using System.Collections.Generic;
using System.Reflection;
using Autofac;
using Csanno.Internal;

namespace Csanno
{

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
            var fallbackAssemblies = new List<Assembly>();

            foreach (var assembly in assemblies)
            {
                // 优先使用编译期生成的代码（按程序集逐个处理）
                if (!TryRegisterGenerated(builder, assembly))
                {
                    fallbackAssemblies.Add(assembly);
                }
            }

            // 对未命中生成器的程序集回退到运行时扫描
            if (fallbackAssemblies.Count > 0)
            {
                var registrations = ComponentScanner.Scan(fallbackAssemblies);

                foreach (var registration in registrations)
                {
                    RegisterComponent(builder, registration);
                }
            }

            return builder;
        }

        /// <summary>
        /// 尝试使用编译期生成的注册代码
        /// </summary>
        /// <param name="builder">容器构建器</param>
        /// <param name="assemblies">要注册的程序集</param>
        /// <returns>如果成功使用生成器注册则为 true，否则为 false</returns>
        private static bool TryRegisterGenerated(ContainerBuilder builder, Assembly assembly)
        {
            var registrationType = assembly.GetType(
                "Csanno.ComponentRegistration.ComponentRegistrationExtensions");

            if (registrationType != null)
            {
                var method = registrationType.GetMethod(
                    "RegisterGeneratedComponents",
                    BindingFlags.Static | BindingFlags.Public);

                if (method != null)
                {
                    method.Invoke(null, new object[] { builder });
                    return true;
                }
            }
            return false;
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

        /// <summary>
        /// 注册 AOP 代理类和拦截器（使用编译期生成的代码）
        /// </summary>
        /// <param name="builder">容器构建器</param>
        /// <param name="assemblies">要扫描的程序集</param>
        /// <returns>容器构建器，用于链式调用</returns>
        public static ContainerBuilder RegisterAopProxies(
            this ContainerBuilder builder,
            params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                TryRegisterAopGenerated(builder, assembly);
            }
            return builder;
        }

        /// <summary>
        /// 注册调用程序集中的 AOP 代理类和拦截器
        /// </summary>
        /// <param name="builder">容器构建器</param>
        /// <returns>容器构建器，用于链式调用</returns>
        public static ContainerBuilder RegisterAopProxies(this ContainerBuilder builder)
        {
            var callingAssembly = Assembly.GetCallingAssembly();
            return builder.RegisterAopProxies(callingAssembly);
        }

        /// <summary>
        /// 尝试使用编译期生成的 AOP 注册代码
        /// </summary>
        private static bool TryRegisterAopGenerated(ContainerBuilder builder, Assembly assembly)
        {
            var registrationType = assembly.GetType(
                "Csanno.ComponentRegistration.AopRegistrationExtensions");

            if (registrationType != null)
            {
                var method = registrationType.GetMethod(
                    "RegisterAopProxies",
                    BindingFlags.Static | BindingFlags.Public);

                if (method != null)
                {
                    method.Invoke(null, new object[] { builder });
                    return true;
                }
            }
            return false;
        }
    }

}
