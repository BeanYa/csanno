using Autofac;

namespace Csanno.Tests;

/// <summary>
/// 容器构建共享辅助类
/// </summary>
public static class ContainerFixture
{
    /// <summary>
    /// 创建并构建一个预配置了测试组件的容器
    /// </summary>
    public static IContainer CreateContainer(Action<ContainerBuilder> configure = null)
    {
        var builder = new ContainerBuilder();

        // 注册测试组件所在的程序集
        builder.RegisterComponents(typeof(ContainerFixture).Assembly);

        // 应用自定义配置
        configure?.Invoke(builder);

        return builder.Build();
    }

    /// <summary>
    /// 创建一个容器构建器，预配置测试组件
    /// </summary>
    public static ContainerBuilder CreateBuilder()
    {
        var builder = new ContainerBuilder();
        builder.RegisterComponents(typeof(ContainerFixture).Assembly);
        return builder;
    }
}
