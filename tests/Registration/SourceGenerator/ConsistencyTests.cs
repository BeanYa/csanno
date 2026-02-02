using System.Reflection;
using Autofac;
using Autofac.Features.Metadata;
using Csanno.Tests.Registration.Lifetime;
using Csanno.Tests.Registration.Metadata;
using NUnit.Framework;
using InternalComponentRegistration = Csanno.Internal.ComponentRegistration;
using InternalInstanceLifetime = Csanno.Internal.InstanceLifetime;

namespace Csanno.Tests
{
    /// <summary>
    /// 测试 Source Generator 与运行时扫描的一致性
    /// </summary>
    [TestFixture]
    public class ConsistencyTests
    {
        private static readonly Assembly TestAssembly = typeof(ConsistencyTests).Assembly;

        #region 辅助方法

        /// <summary>
        /// 通过运行时扫描创建容器
        /// </summary>
        private static IContainer CreateContainerViaRuntimeScan()
        {
            var builder = new ContainerBuilder();
            var registrations = GetRuntimeScanRegistrations();

            foreach (var reg in registrations)
            {
                ApplyRegistration(builder, reg);
            }

            return builder.Build();
        }

        /// <summary>
        /// 通过 Source Generator 创建容器（使用 ContainerFixture）
        /// </summary>
        private static IContainer CreateContainerViaSourceGenerator()
        {
            return ContainerFixture.CreateContainer();
        }

        /// <summary>
        /// 强制调用 ComponentScanner.Scan 获取运行时扫描结果
        /// </summary>
        private static IEnumerable<InternalComponentRegistration> GetRuntimeScanRegistrations()
        {
            var scannerType = typeof(RegistrationExtensions).Assembly
                .GetType("Csanno.Internal.ComponentScanner");
            Assert.That(scannerType, Is.Not.Null, "ComponentScanner 类型应存在");

            var scanMethod = scannerType!.GetMethod("Scan", BindingFlags.Static | BindingFlags.Public);
            Assert.That(scanMethod, Is.Not.Null, "ComponentScanner.Scan 方法应存在");

            var result = scanMethod!.Invoke(null, [new[] { TestAssembly }]);
            return (IEnumerable<InternalComponentRegistration>)result!;
        }

        /// <summary>
        /// 应用 ComponentRegistration 到 ContainerBuilder
        /// </summary>
        private static void ApplyRegistration(ContainerBuilder builder, InternalComponentRegistration reg)
        {
            var registration = builder.RegisterType(reg.ComponentType);

            // 应用服务类型
            foreach (var serviceType in reg.ServiceTypes)
            {
                if (serviceType != reg.ComponentType)
                {
                    registration.As(serviceType);
                }
            }

            // 应用生命周期
            switch (reg.Lifetime)
            {
                case InternalInstanceLifetime.Singleton:
                    registration.SingleInstance();
                    break;
                case InternalInstanceLifetime.Scoped:
                    registration.InstancePerLifetimeScope();
                    break;
                case InternalInstanceLifetime.PerMatchingLifetimeScope:
                    if (reg.LifetimeScopeTags?.Length > 0)
                    {
                        registration.InstancePerMatchingLifetimeScope(reg.LifetimeScopeTags);
                    }
                    break;
                case InternalInstanceLifetime.PerRequest:
                    registration.InstancePerRequest();
                    break;
                case InternalInstanceLifetime.Transient:
                default:
                    registration.InstancePerDependency();
                    break;
            }

            // 应用元数据
            if (reg.Metadata is not null)
            {
                foreach (var (key, value) in reg.Metadata)
                {
                    registration.WithMetadata(key, value);
                }
            }
        }

        #endregion

        #region 1. 生命周期行为一致性测试

        /// <summary>
        /// 测试用例：Singleton 生命周期行为一致
        /// </summary>
        [Test]
        public void Singleton_Lifetime_Behavior_Should_Be_Consistent()
        {
            // Arrange
            using var sgContainer = CreateContainerViaSourceGenerator();
            using var rtContainer = CreateContainerViaRuntimeScan();

            // Act & Assert - 两个容器中 Singleton 都应返回同一实例
            var sgInstance1 = sgContainer.Resolve<SingletonComponent>();
            var sgInstance2 = sgContainer.Resolve<SingletonComponent>();
            Assert.That(sgInstance1, Is.SameAs(sgInstance2), "SG: Singleton 应返回同一实例");

            var rtInstance1 = rtContainer.Resolve<SingletonComponent>();
            var rtInstance2 = rtContainer.Resolve<SingletonComponent>();
            Assert.That(rtInstance1, Is.SameAs(rtInstance2), "RT: Singleton 应返回同一实例");
        }

        /// <summary>
        /// 测试用例：Scoped 生命周期行为一致
        /// </summary>
        [Test]
        public void Scoped_Lifetime_Behavior_Should_Be_Consistent()
        {
            // Arrange
            using var sgContainer = CreateContainerViaSourceGenerator();
            using var rtContainer = CreateContainerViaRuntimeScan();

            // Act & Assert - 同一作用域内返回同一实例
            using var sgScope = sgContainer.BeginLifetimeScope();
            var sgInstance1 = sgScope.Resolve<ScopedComponent>();
            var sgInstance2 = sgScope.Resolve<ScopedComponent>();
            Assert.That(sgInstance1, Is.SameAs(sgInstance2), "SG: Scoped 同一作用域应返回同一实例");

            using var rtScope = rtContainer.BeginLifetimeScope();
            var rtInstance1 = rtScope.Resolve<ScopedComponent>();
            var rtInstance2 = rtScope.Resolve<ScopedComponent>();
            Assert.That(rtInstance1, Is.SameAs(rtInstance2), "RT: Scoped 同一作用域应返回同一实例");

            // 不同作用域应返回不同实例
            using var sgScope2 = sgContainer.BeginLifetimeScope();
            var sgInstance3 = sgScope2.Resolve<ScopedComponent>();
            Assert.That(sgInstance1, Is.Not.SameAs(sgInstance3), "SG: Scoped 不同作用域应返回不同实例");

            using var rtScope2 = rtContainer.BeginLifetimeScope();
            var rtInstance3 = rtScope2.Resolve<ScopedComponent>();
            Assert.That(rtInstance1, Is.Not.SameAs(rtInstance3), "RT: Scoped 不同作用域应返回不同实例");
        }

        /// <summary>
        /// 测试用例：Transient 生命周期行为一致
        /// </summary>
        [Test]
        public void Transient_Lifetime_Behavior_Should_Be_Consistent()
        {
            // Arrange
            using var sgContainer = CreateContainerViaSourceGenerator();
            using var rtContainer = CreateContainerViaRuntimeScan();

            // Act & Assert - 每次解析都返回新实例
            var sgInstance1 = sgContainer.Resolve<TransientComponent>();
            var sgInstance2 = sgContainer.Resolve<TransientComponent>();
            Assert.That(sgInstance1, Is.Not.SameAs(sgInstance2), "SG: Transient 应每次返回新实例");

            var rtInstance1 = rtContainer.Resolve<TransientComponent>();
            var rtInstance2 = rtContainer.Resolve<TransientComponent>();
            Assert.That(rtInstance1, Is.Not.SameAs(rtInstance2), "RT: Transient 应每次返回新实例");
        }

        #endregion

        #region 2. 服务类型映射一致性测试

        /// <summary>
        /// 测试用例：核心生命周期组件的服务类型映射一致
        /// </summary>
        [Test]
        public void Service_Type_Mapping_Should_Be_Consistent()
        {
            // Arrange - 使用已知在两种路径都能注册的核心组件
            using var sgContainer = CreateContainerViaSourceGenerator();
            using var rtContainer = CreateContainerViaRuntimeScan();

            // 核心生命周期组件类型
            var coreTypes = new[]
            {
                typeof(SingletonComponent),
                typeof(ScopedComponent),
                typeof(TransientComponent)
            };

            // Act & Assert
            foreach (var type in coreTypes)
            {
                var canResolveSg = sgContainer.IsRegistered(type);
                var canResolveRt = rtContainer.IsRegistered(type);

                Assert.That(canResolveSg, Is.True, $"SG 应能解析 {type.Name}");
                Assert.That(canResolveRt, Is.True, $"RT 应能解析 {type.Name}");
            }
        }

        /// <summary>
        /// 测试用例：组件通过接口注册一致
        /// </summary>
        [Test]
        public void Component_Interface_Registration_Should_Be_Consistent()
        {
            // Arrange
            using var sgContainer = CreateContainerViaSourceGenerator();
            using var rtContainer = CreateContainerViaRuntimeScan();

            // Act & Assert - 验证通过接口注册的组件
            var sgCanResolve = sgContainer.IsRegistered<IComponentWithMetadata>();
            var rtCanResolve = rtContainer.IsRegistered<IComponentWithMetadata>();

            Assert.That(sgCanResolve, Is.EqualTo(rtCanResolve),
                $"IComponentWithMetadata 的注册状态应一致 (SG={sgCanResolve}, RT={rtCanResolve})");
        }

        #endregion

        #region 3. Metadata 一致性测试

        /// <summary>
        /// 测试用例：运行时扫描的元数据与 SG 元数据一致
        /// </summary>
        [Test]
        public void Metadata_Should_Be_Consistent()
        {
            // Arrange - 获取运行时扫描中有元数据的注册
            var runtimeRegistrations = GetRuntimeScanRegistrations()
                .Where(r => r.Metadata is not null && r.Metadata.Count > 0)
                .ToList();

            // 从生成的代码中提取元数据
            var generatedCode = ReadGeneratedRegistrationCode();

            // Act & Assert - 验证带元数据的组件在生成代码中有对应的 WithMetadata
            foreach (var reg in runtimeRegistrations)
            {
                var typeName = reg.ComponentType.FullName;
                Assert.That(generatedCode, Does.Contain($"builder.RegisterType<{typeName}>"),
                    $"组件 {typeName} 应在生成代码中注册");

                foreach (var (key, _) in reg.Metadata!)
                {
                    Assert.That(generatedCode, Does.Contain($".WithMetadata(\"{key}\""),
                        $"组件 {reg.ComponentType.Name} 应有元数据键 '{key}'");
                }
            }
        }

        /// <summary>
        /// 测试用例：ComponentWithMetadata 用户定义的元数据值一致
        /// </summary>
        [Test]
        public void ComponentWithMetadata_Should_Have_Consistent_Metadata()
        {
            // Arrange
            using var sgContainer = CreateContainerViaSourceGenerator();
            using var rtContainer = CreateContainerViaRuntimeScan();

            // Act - 解析通过接口注册的组件
            var sgMeta = sgContainer.Resolve<Meta<IComponentWithMetadata>>();
            var rtMeta = rtContainer.Resolve<Meta<IComponentWithMetadata>>();

            // 排除系统生成的元数据键（如 __RegistrationOrder）
            var userDefinedKeys = sgMeta.Metadata.Keys
                .Where(k => !k.StartsWith("__"))
                .ToList();

            // Assert - 验证用户定义的元数据
            foreach (var key in userDefinedKeys)
            {
                Assert.That(rtMeta.Metadata.ContainsKey(key), Is.True,
                    $"RT 应包含元数据键 '{key}'");
                Assert.That(sgMeta.Metadata[key], Is.EqualTo(rtMeta.Metadata[key]),
                    $"元数据 '{key}' 的值应一致");
            }
        }

        #endregion

        #region 4. 综合一致性测试

        /// <summary>
        /// 测试用例：核心生命周期组件都能通过两种路径解析
        /// </summary>
        [Test]
        public void Core_Components_Should_Be_Resolvable_Via_Both_Paths()
        {
            // Arrange
            using var sgContainer = CreateContainerViaSourceGenerator();
            using var rtContainer = CreateContainerViaRuntimeScan();

            // 核心生命周期组件（这些组件注册为自身类型）
            var coreTypes = new[]
            {
                typeof(SingletonComponent),
                typeof(ScopedComponent),
                typeof(TransientComponent)
            };

            // Act & Assert
            foreach (var type in coreTypes)
            {
                Assert.That(sgContainer.IsRegistered(type), Is.True,
                    $"SG 容器应能解析组件 {type.Name}");
                Assert.That(rtContainer.IsRegistered(type), Is.True,
                    $"RT 容器应能解析组件 {type.Name}");
            }
        }

        /// <summary>
        /// 测试用例：生成代码包含核心组件注册
        /// </summary>
        [Test]
        public void Generated_Code_Should_Contain_Core_Components()
        {
            // Arrange - 核心组件类型全名
            var coreComponents = new[]
            {
                "Csanno.Tests.Registration.Lifetime.SingletonComponent",
                "Csanno.Tests.Registration.Lifetime.ScopedComponent",
                "Csanno.Tests.Registration.Lifetime.TransientComponent"
            };

            // Act
            var generatedCode = ReadGeneratedRegistrationCode();

            // Assert
            foreach (var component in coreComponents)
            {
                Assert.That(generatedCode, Does.Contain($"builder.RegisterType<{component}>"),
                    $"生成代码应包含 {component}");
            }
        }

        #endregion

        #region 辅助方法 - 读取生成代码

        private static string ReadGeneratedRegistrationCode()
        {
            var fileName = "ComponentRegistration.Csanno.Tests.g.cs";
            var relativePath = Path.Combine("Generated", "Csanno.Generator",
                "Csanno.Generator.ComponentGenerator", fileName);

            return ReadGeneratedFile(relativePath);
        }

        private static string ReadGeneratedFile(string relativePath)
        {
            var currentDir = Directory.GetCurrentDirectory();
            var assemblyLocation = Path.GetDirectoryName(typeof(ConsistencyTests).Assembly.Location) ?? currentDir;
            var searchPaths = new List<string>();

            var testsFolderFromAssembly = Path.GetFullPath(Path.Combine(assemblyLocation, "..", "..", ".."));
            searchPaths.Add(Path.Combine(testsFolderFromAssembly, "obj", relativePath));

            var dir = new DirectoryInfo(currentDir);
            while (dir != null)
            {
                if (dir.Name.Equals("tests", StringComparison.OrdinalIgnoreCase))
                {
                    searchPaths.Add(Path.Combine(dir.FullName, "obj", relativePath));
                }
                var testsDir = Path.Combine(dir.FullName, "tests");
                if (Directory.Exists(testsDir))
                {
                    searchPaths.Add(Path.Combine(testsDir, "obj", relativePath));
                }
                dir = dir.Parent;
            }

            searchPaths.Add(Path.Combine(currentDir, "tests", "obj", relativePath));
            searchPaths.Add(Path.Combine(currentDir, "..", "tests", "obj", relativePath));

            foreach (var path in searchPaths)
            {
                if (File.Exists(path))
                {
                    return File.ReadAllText(path);
                }
            }

            throw new FileNotFoundException($"Could not find generated file '{relativePath}'");
        }

        #endregion
    }
}
