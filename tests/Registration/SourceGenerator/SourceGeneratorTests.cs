using NUnit.Framework;
using System.Reflection;
using System.Text.RegularExpressions;
using Autofac;
using Csanno.Tests.Registration.Lifetime;
using Csanno.Tests.Registration.Services;

namespace Csanno.Tests
{

    /// <summary>
    /// Source Generator 生成的代码测试
    /// </summary>
    [TestFixture]
    public class SourceGeneratorTests
    {
        private const string DependenciesNamespace = "Csanno.Tests.Registration.Dependencies";
        private const string EdgeCasesNamespace = "Csanno.Tests.Registration.EdgeCases";
        private const string LifetimeNamespace = "Csanno.Tests.Registration.Lifetime";
        private const string MetadataNamespace = "Csanno.Tests.Registration.Metadata";
        private const string OwnedNamespace = "Csanno.Tests.Registration.Owned";
        private const string ServicesNamespace = "Csanno.Tests.Registration.Services";

        #region 1. 生成器加载测试

        /// <summary>
        /// 测试用例：验证 Source Generator 已被加载
        /// </summary>
        [Test]
        public void Generator_Should_Be_Loaded()
        {
            // Arrange & Act
            var generatorLoadedType = Type.GetType("Csanno.Generated.GeneratorLoaded");

            // Assert
            Assert.That(generatorLoadedType, Is.Not.Null, "GeneratorLoaded 类型应该存在");

            var isLoadedField = generatorLoadedType?.GetField("IsLoaded", BindingFlags.Static | BindingFlags.Public);
            Assert.That(isLoadedField, Is.Not.Null, "IsLoaded 字段应该存在");

            var isLoaded = (bool?)(isLoadedField?.GetValue(null));
            Assert.That(isLoaded, Is.True, "Source Generator 应该已被加载");
        }

        /// <summary>
        /// 测试用例：验证生成器加载时间戳有效
        /// </summary>
        [Test]
        public void Generator_Should_Have_Valid_Timestamp()
        {
            // Arrange & Act
            var generatorLoadedType = Type.GetType("Csanno.Generated.GeneratorLoaded");
            var loadedAtField = generatorLoadedType?.GetField("LoadedAt", BindingFlags.Static | BindingFlags.Public);

            // Assert
            Assert.That(loadedAtField, Is.Not.Null, "LoadedAt 字段应该存在");

            var loadedAt = (string?)(loadedAtField?.GetValue(null));
            Assert.That(loadedAt, Is.Not.Null.And.Not.Empty, "LoadedAt 应该包含时间戳");

            // 验证是有效的 ISO 8601 格式
            Assert.That(() => DateTime.Parse(loadedAt!), Throws.Nothing, "LoadedAt 应该是有效的日期时间格式");
        }

        #endregion

        #region 2. 生成的注册代码存在性测试

        /// <summary>
        /// 测试用例：验证生成的注册扩展类存在
        /// </summary>
        [Test]
        public void RegistrationExtensions_Type_Should_Exist()
        {
            // Arrange
            var assembly = typeof(SourceGeneratorTests).Assembly;

            // Act
            var registrationExtensionsType = assembly.GetType(
                "Csanno.ComponentRegistration.ComponentRegistrationExtensions");

            // Assert
            Assert.That(registrationExtensionsType, Is.Not.Null,
                "ComponentRegistrationExtensions 类型应该存在");
            Assert.That(registrationExtensionsType?.IsClass, Is.True,
                "ComponentRegistrationExtensions 应该是类");
        }

        /// <summary>
        /// 测试用例：验证 RegisterGeneratedComponents 方法存在
        /// </summary>
        [Test]
        public void RegisterGeneratedComponents_Method_Should_Exist()
        {
            // Arrange
            var assembly = typeof(SourceGeneratorTests).Assembly;
            var registrationExtensionsType = assembly.GetType(
                "Csanno.ComponentRegistration.ComponentRegistrationExtensions");

            // Act
            var registerMethod = registrationExtensionsType?.GetMethod(
                "RegisterGeneratedComponents",
                BindingFlags.Static | BindingFlags.Public);

            // Assert
            Assert.That(registerMethod, Is.Not.Null,
                "RegisterGeneratedComponents 方法应该存在");
            Assert.That(registerMethod?.IsStatic, Is.True,
                "RegisterGeneratedComponents 应该是静态方法");
            Assert.That(registerMethod?.ReturnType.Name, Is.EqualTo("Void"),
                "RegisterGeneratedComponents 应该返回 void");
        }

        /// <summary>
        /// 测试用例：验证 RegisterGeneratedComponents 方法签名正确
        /// </summary>
        [Test]
        public void RegisterGeneratedComponents_Should_Have_Correct_Signature()
        {
            // Arrange
            var assembly = typeof(SourceGeneratorTests).Assembly;
            var registrationExtensionsType = assembly.GetType(
                "Csanno.ComponentRegistration.ComponentRegistrationExtensions");
            var registerMethod = registrationExtensionsType?.GetMethod(
                "RegisterGeneratedComponents",
                BindingFlags.Static | BindingFlags.Public);

            // Act
            var parameters = registerMethod?.GetParameters();

            // Assert
            Assert.That(parameters, Is.Not.Null.And.Length.EqualTo(1),
                "RegisterGeneratedComponents 应该有一个参数");
            Assert.That(parameters![0].ParameterType, Is.EqualTo(typeof(ContainerBuilder)),
                "参数类型应该是 ContainerBuilder");
            Assert.That(parameters[0].Name, Is.EqualTo("builder"),
                "参数名称应该是 builder");
        }

        #endregion

        #region 3. 生成的代码完整性测试

        /// <summary>
        /// 测试用例：验证所有预期的组件都在生成的代码中被注册
        /// </summary>
        [Test]
        public void Generated_Code_Should_Register_All_Expected_Components()
        {
            // Arrange - 定义预期的所有测试组件及其命名空间
            var expectedComponents = new Dictionary<string, string>
            {
                ["ComponentWithMetadata"] = MetadataNamespace,
                ["ComponentWithMultipleMetadataTypes"] = MetadataNamespace,
                ["ConflictingLifetimeComponent"] = EdgeCasesNamespace,
                ["Consumer"] = DependenciesNamespace,
                ["ConsumerWithMissingDependency"] = EdgeCasesNamespace,
                ["DefaultLifetimeComponent"] = EdgeCasesNamespace,
                ["DisposableOwnedComponent"] = OwnedNamespace,
                ["MatchingScopeComponent"] = OwnedNamespace,
                ["MultiServiceComponent"] = ServicesNamespace,
                ["MultiTagScopeComponent"] = OwnedNamespace,
                ["OwnedComponent"] = OwnedNamespace,
                ["ScopedComponent"] = LifetimeNamespace,
                ["ServiceWithInterface"] = ServicesNamespace,
                ["SimpleComponent"] = LifetimeNamespace,
                ["SingletonComponent"] = LifetimeNamespace,
                ["TopLevelConsumer"] = DependenciesNamespace,
                ["TransientComponent"] = LifetimeNamespace
            };

            // Act - 读取生成的代码文件
            var generatedCode = ReadGeneratedRegistrationCode();

            // Assert - 验证每个组件都在生成代码中
            foreach (var (component, ns) in expectedComponents)
            {
                Assert.That(generatedCode, Does.Contain($"builder.RegisterType<{ns}.{component}>"),
                    $"生成的代码应该包含 {ns}.{component} 的注册");
            }
        }

        /// <summary>
        /// 测试用例：验证生成的代码包含自动生成标记
        /// </summary>
        [Test]
        public void Generated_Code_Should_Contain_Auto_Generated_Header()
        {
            // Arrange & Act
            var generatedCode = ReadGeneratedRegistrationCode();

            // Assert
            Assert.That(generatedCode, Does.Contain("// <auto-generated/>"),
                "生成的代码应该包含 <auto-generated/> 标记");
        }

        /// <summary>
        /// 测试用例：验证生成的代码包含调试信息
        /// </summary>
        [Test]
        public void Generated_Code_Should_Contain_Debug_Info()
        {
            // Arrange & Act
            var debugCode = ReadGeneratedDebugCode();

            // Assert
            Assert.That(debugCode, Is.Not.Null.And.Not.Empty,
                "调试信息文件应该存在且不为空");
            Assert.That(debugCode, Does.Contain("// Source Generator Debug Info"),
                "调试信息应该包含标题");
            Assert.That(debugCode, Does.Contain("// Valid components:"),
                "调试信息应该显示有效组件数量");
        }

        #endregion

        #region 4. 生命周期正确性测试

        /// <summary>
        /// 测试用例：验证 Transient 组件使用 InstancePerDependency
        /// </summary>
        [Test]
        public void Transient_Component_Should_Use_InstancePerDependency()
        {
            // Arrange & Act
            var generatedCode = ReadGeneratedRegistrationCode();

            // Assert
            Assert.That(generatedCode, Does.Contain($"builder.RegisterType<{LifetimeNamespace}.TransientComponent>().InstancePerDependency()"),
                "TransientComponent 应该使用 InstancePerDependency");
        }

        /// <summary>
        /// 测试用例：验证 Scoped 组件使用 InstancePerLifetimeScope
        /// </summary>
        [Test]
        public void Scoped_Component_Should_Use_InstancePerLifetimeScope()
        {
            // Arrange & Act
            var generatedCode = ReadGeneratedRegistrationCode();

            // Assert
            Assert.That(generatedCode, Does.Contain($"builder.RegisterType<{LifetimeNamespace}.ScopedComponent>().InstancePerLifetimeScope()"),
                "ScopedComponent 应该使用 InstancePerLifetimeScope");
        }

        /// <summary>
        /// 测试用例：验证 Singleton 组件使用 SingleInstance
        /// </summary>
        [Test]
        public void Singleton_Component_Should_Use_SingleInstance()
        {
            // Arrange & Act
            var generatedCode = ReadGeneratedRegistrationCode();

            // Assert
            Assert.That(generatedCode, Does.Contain($"builder.RegisterType<{LifetimeNamespace}.SingletonComponent>().SingleInstance()"),
                "SingletonComponent 应该使用 SingleInstance");
        }

        /// <summary>
        /// 测试用例：验证 Owned 组件使用 InstancePerOwned
        /// </summary>
        [Test]
        public void Owned_Component_Should_Use_InstancePerOwned()
        {
            // Arrange & Act
            var generatedCode = ReadGeneratedRegistrationCode();

            // Assert
            Assert.That(generatedCode, Does.Contain($"builder.RegisterType<{OwnedNamespace}.OwnedComponent>().InstancePerOwned<{OwnedNamespace}.OwnedComponent>()"),
                "OwnedComponent 应该使用 InstancePerOwned");
        }

        /// <summary>
        /// 测试用例：验证 DisposableOwned 组件使用 InstancePerOwned
        /// </summary>
        [Test]
        public void DisposableOwned_Component_Should_Use_InstancePerOwned()
        {
            // Arrange & Act
            var generatedCode = ReadGeneratedRegistrationCode();

            // Assert
            Assert.That(generatedCode, Does.Contain($"builder.RegisterType<{OwnedNamespace}.DisposableOwnedComponent>().InstancePerOwned<{OwnedNamespace}.DisposableOwnedComponent>()"),
                "DisposableOwnedComponent 应该使用 InstancePerOwned");
        }

        /// <summary>
        /// 测试用例：验证 PerMatchingLifetimeScope 组件使用正确方法
        /// </summary>
        [Test]
        public void PerMatchingLifetimeScope_Component_Should_Use_InstancePerMatchingLifetimeScope()
        {
            // Arrange & Act
            var generatedCode = ReadGeneratedRegistrationCode();

            // Assert
            Assert.That(generatedCode, Does.Contain($"builder.RegisterType<{OwnedNamespace}.MatchingScopeComponent>().InstancePerMatchingLifetimeScope(\"request\")"),
                "MatchingScopeComponent 应该使用 InstancePerMatchingLifetimeScope(\"request\")");
            Assert.That(generatedCode, Does.Contain($"builder.RegisterType<{OwnedNamespace}.MultiTagScopeComponent>().InstancePerMatchingLifetimeScope(\"tag1\", \"tag2\")"),
                "MultiTagScopeComponent 应该支持多个标签");
        }

        #endregion

        #region 5. 服务接口映射测试

        /// <summary>
        /// 测试用例：验证组件可以注册为单个接口
        /// </summary>
        [Test]
        public void Component_Should_Register_As_Single_Interface()
        {
            // Arrange & Act
            var generatedCode = ReadGeneratedRegistrationCode();

            // Assert
            Assert.That(generatedCode, Does.Contain($".As<{ServicesNamespace}.IService>()"),
                "ServiceWithInterface 应该注册为 IService 接口");
        }

        /// <summary>
        /// 测试用例：验证组件可以注册为多个接口
        /// </summary>
        [Test]
        public void Component_Should_Register_As_Multiple_Interfaces()
        {
            // Arrange & Act
            var generatedCode = ReadGeneratedRegistrationCode();

            // Assert
            Assert.That(generatedCode, Does.Contain($"builder.RegisterType<{ServicesNamespace}.MultiServiceComponent>().InstancePerDependency()"),
                "MultiServiceComponent 的注册应该存在");

            // 验证包含两个接口映射
            var service1 = generatedCode.Contains($".As<{ServicesNamespace}.IService1>()");
            var service2 = generatedCode.Contains($".As<{ServicesNamespace}.IService2>()");
            Assert.That(service1 && service2, Is.True,
                "MultiServiceComponent 应该同时注册为 IService1 和 IService2");
        }

        #endregion

        #region 6. 元数据注册测试

        /// <summary>
        /// 测试用例：验证元数据注册正确（字符串、整数类型）
        /// </summary>
        [Test]
        public void Component_Should_Register_Metadata()
        {
            // Arrange & Act
            var generatedCode = ReadGeneratedRegistrationCode();

            // Assert
            Assert.That(generatedCode, Does.Contain(".WithMetadata(\"Name\", \"TestComponent\")"),
                "应该包含 Name 元数据");
            Assert.That(generatedCode, Does.Contain(".WithMetadata(\"Version\", 1)"),
                "应该包含 Version 元数据");
        }

        /// <summary>
        /// 测试用例：验证多种元数据类型注册（字符串、整数、布尔）
        /// </summary>
        [Test]
        public void Component_Should_Register_Multiple_Metadata_Types()
        {
            // Arrange & Act
            var generatedCode = ReadGeneratedRegistrationCode();

            // Assert
            Assert.That(generatedCode, Does.Contain(".WithMetadata(\"StringKey\", \"string value\")"),
                "应该包含 StringKey 元数据");
            Assert.That(generatedCode, Does.Contain(".WithMetadata(\"IntKey\", 42)"),
                "应该包含 IntKey 元数据");
            Assert.That(generatedCode, Does.Contain(".WithMetadata(\"BoolKey\", true)"),
                "应该包含 BoolKey 元数据");
        }

        #endregion

        #region 7. 生成代码可用性测试

        /// <summary>
        /// 测试用例：验证直接调用生成的注册方法能成功注册所有组件
        /// </summary>
        [Test]
        public void RegisterGeneratedComponents_Should_Register_All_Components_Successfully()
        {
            // Arrange
            var builder = new ContainerBuilder();
            var assembly = typeof(SourceGeneratorTests).Assembly;
            var registrationExtensionsType = assembly.GetType(
                "Csanno.ComponentRegistration.ComponentRegistrationExtensions");
            var registerMethod = registrationExtensionsType?.GetMethod(
                "RegisterGeneratedComponents",
                BindingFlags.Static | BindingFlags.Public);

            // Act
            registerMethod?.Invoke(null, new object[] { builder });
            var container = builder.Build();

            // Assert - 验证所有基础组件都能解析
            Assert.That(() => container.Resolve<SimpleComponent>(), Throws.Nothing,
                "SimpleComponent 应该能解析");
            Assert.That(() => container.Resolve<TransientComponent>(), Throws.Nothing,
                "TransientComponent 应该能解析");
            Assert.That(() => container.Resolve<ScopedComponent>(), Throws.Nothing,
                "ScopedComponent 应该能解析");
            Assert.That(() => container.Resolve<SingletonComponent>(), Throws.Nothing,
                "SingletonComponent 应该能解析");
        }

        /// <summary>
        /// 测试用例：验证通过生成的代码注册后组件行为符合预期
        /// </summary>
        [Test]
        public void Generated_Code_Should_Register_Components_With_Correct_Behavior()
        {
            // Arrange
            var builder = new ContainerBuilder();
            var assembly = typeof(SourceGeneratorTests).Assembly;
            var registrationExtensionsType = assembly.GetType(
                "Csanno.ComponentRegistration.ComponentRegistrationExtensions");
            var registerMethod = registrationExtensionsType?.GetMethod(
                "RegisterGeneratedComponents",
                BindingFlags.Static | BindingFlags.Public);

            // Act
            registerMethod?.Invoke(null, new object[] { builder });
            var container = builder.Build();

            // Assert - SimpleComponent 行为正确
            var simpleComponent = container.Resolve<SimpleComponent>();
            Assert.That(simpleComponent, Is.Not.Null);
            Assert.That(simpleComponent.Greet(), Is.EqualTo("Hello from SimpleComponent"));

            // Assert - Singleton 确实是单例
            var singleton1 = container.Resolve<SingletonComponent>();
            var singleton2 = container.Resolve<SingletonComponent>();
            Assert.That(singleton1, Is.SameAs(singleton2), "Singleton 应该是单例");
        }

        /// <summary>
        /// 测试用例：验证生成的代码注册的服务能通过接口解析
        /// </summary>
        [Test]
        public void Generated_Code_Should_Register_Services_That_Can_Be_Resolved_By_Interface()
        {
            // Arrange
            var builder = new ContainerBuilder();
            var assembly = typeof(SourceGeneratorTests).Assembly;
            var registrationExtensionsType = assembly.GetType(
                "Csanno.ComponentRegistration.ComponentRegistrationExtensions");
            var registerMethod = registrationExtensionsType?.GetMethod(
                "RegisterGeneratedComponents",
                BindingFlags.Static | BindingFlags.Public);

            // Act
            registerMethod?.Invoke(null, new object[] { builder });
            var container = builder.Build();

            // Assert - 通过接口解析
            var service = container.Resolve<IService>();
            Assert.That(service, Is.Not.Null, "应该能通过 IService 接口解析");
            Assert.That(service.DoWork(), Is.EqualTo("Work done"),
                "服务行为应该正确");

            var multiService1 = container.Resolve<IService1>();
            var multiService2 = container.Resolve<IService2>();
            Assert.That(multiService1, Is.Not.Null, "应该能通过 IService1 接口解析");
            Assert.That(multiService2, Is.Not.Null, "应该能通过 IService2 接口解析");
        }

        #endregion

        #region 8. 与运行时扫描的一致性测试

        /// <summary>
        /// 测试用例：验证生成的代码与预期的组件列表一致
        /// </summary>
        [Test]
        public void Generated_Code_Should_Match_Expected_Component_List()
        {
            // Arrange - 定义预期的所有测试组件
            var expectedComponents = new[]
            {
                "ComponentWithMetadata",
                "ComponentWithMultipleMetadataTypes",
                "ConflictingLifetimeComponent",
                "Consumer",
                "ConsumerWithMissingDependency",
                "DefaultLifetimeComponent",
                "DisposableOwnedComponent",
                "MatchingScopeComponent",
                "MultiServiceComponent",
                "MultiTagScopeComponent",
                "OwnedComponent",
                "ScopedComponent",
                "ServiceWithInterface",
                "SimpleComponent",
                "SingletonComponent",
                "TopLevelConsumer",
                "TransientComponent"
            };

            // Act - 从生成的代码中提取注册的组件类型
            var generatedCode = ReadGeneratedRegistrationCode();
            var generatedComponentTypes = new HashSet<string>();
            foreach (Match match in Regex.Matches(
                generatedCode,
                @"builder\.RegisterType<[^>]*\.([A-Za-z0-9_]+)>"))
            {
                var className = match.Groups[1].Value;
                generatedComponentTypes.Add(className);
            }

            // Assert - 验证生成的代码包含所有预期组件
            foreach (var expectedComponent in expectedComponents)
            {
                Assert.That(generatedComponentTypes, Does.Contain(expectedComponent),
                    $"生成的代码应该包含组件：{expectedComponent}");
            }

            // 验证生成的代码只包含预期组件（不包含不应该注册的组件）
            var unexpectedComponents = new[] { "AbstractComponent", "StaticComponent", "PrivateConstructorComponent" };
            foreach (var unexpectedComponent in unexpectedComponents)
            {
                Assert.That(generatedComponentTypes, Does.Not.Contain(unexpectedComponent),
                    $"生成的代码不应该包含组件：{unexpectedComponent}（应该被排除）");
            }
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 读取生成的注册代码
        /// </summary>
        private static string ReadGeneratedRegistrationCode()
        {
            var fileName = "ComponentRegistration.Csanno.Tests.g.cs";
            var relativePath = Path.Combine("Generated", "Csanno.Generator",
                "Csanno.Generator.ComponentGenerator", fileName);

            return ReadGeneratedFile(relativePath);
        }

        /// <summary>
        /// 读取生成的调试信息代码
        /// </summary>
        private static string ReadGeneratedDebugCode()
        {
            var fileName = "GeneratorDebug.g.cs";
            var relativePath = Path.Combine("Generated", "Csanno.Generator",
                "Csanno.Generator.ComponentGenerator", fileName);

            return ReadGeneratedFile(relativePath);
        }
        
        private static string ReadGeneratedFile(string relativePath)
        {
            var currentDir = Directory.GetCurrentDirectory();
            var assemblyLocation = Path.GetDirectoryName(typeof(SourceGeneratorTests).Assembly.Location) ?? currentDir;
            var searchPaths = new List<string>();

            // Strategy 1: Look relative to Assembly Location (usually bin/Debug/net10.0)
            // From bin/Release/net10.0, up 3 levels to tests/ folder
            var testsFolderFromAssembly = Path.GetFullPath(Path.Combine(assemblyLocation, "..", "..", ".."));
            searchPaths.Add(Path.Combine(testsFolderFromAssembly, "obj", relativePath));

            // Strategy 2: Look upwards from CWD for "tests/obj" or just "obj"
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

            // Strategy 3: Hardcoded relative lookups from CWD
            searchPaths.Add(Path.Combine(currentDir, "tests", "obj", relativePath));
            searchPaths.Add(Path.Combine(currentDir, "..", "tests", "obj", relativePath));

            // Attempt to find the file
            foreach (var path in searchPaths)
            {
                if (File.Exists(path))
                {
                    return File.ReadAllText(path);
                }
            }

            // If we get here, it failed. Construct a detailed error message.
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Could not find generated file '{relativePath}' in any expected location.");
            sb.AppendLine($"Current Directory: {currentDir}");
            sb.AppendLine($"Assembly Location: {assemblyLocation}");
            sb.AppendLine("Attempted paths:");
            foreach (var path in searchPaths.Distinct())
            {
                sb.AppendLine($"  - {path} (Exists: {File.Exists(path)})");
            }
            
            // Also list contents of what we CAN find to help debug
            if (Directory.Exists(testsFolderFromAssembly))
            {
                sb.AppendLine($"Contents of '{testsFolderFromAssembly}':");
                foreach (var d in Directory.GetDirectories(testsFolderFromAssembly))
                {
                    sb.AppendLine($"  [DIR] {Path.GetFileName(d)}");
                }

                var objPath = Path.Combine(testsFolderFromAssembly, "obj");
                if (Directory.Exists(objPath))
                {
                    sb.AppendLine($"Contents of '{objPath}':");
                    foreach (var d in Directory.GetDirectories(objPath))
                    {
                        sb.AppendLine($"  [DIR] {Path.GetFileName(d)}");
                    }
                }
            }

            throw new FileNotFoundException(sb.ToString());
        }

        #endregion
    }
}
