using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Csanno.Generator.Emit;
using Csanno.Generator.Models;

namespace Csanno.Generator
{

    /// <summary>
    /// AOP 代理类生成器
    /// 扫描拦截器和需要拦截的类，生成代理类代码
    /// </summary>
    [Generator]
    public sealed class InterceptorGenerator : IIncrementalGenerator
    {
        private static readonly DiagnosticDescriptor AsyncMethodNotSupported = new(
            id: "CSANNO001",
            title: "Async method not supported for interception",
            messageFormat: "Method '{0}' is async and cannot be intercepted",
            category: "Csanno.Generator",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor RefOutParameterNotSupported = new(
            id: "CSANNO002",
            title: "Ref/Out parameter not supported for interception",
            messageFormat: "Method '{0}' has ref or out parameters and cannot be intercepted",
            category: "Csanno.Generator",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor GenericMethodNotSupported = new(
            id: "CSANNO003",
            title: "Generic method not supported for interception",
            messageFormat: "Method '{0}' is generic and cannot be intercepted",
            category: "Csanno.Generator",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // 1. 扫描所有带 [BindWith] 特性的拦截器类
            var interceptorDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (node, _) => node is ClassDeclarationSyntax cds && cds.AttributeLists.Count > 0,
                    transform: static (ctx, _) => TryGetInterceptorInfo(ctx)
                )
                .Where(static m => m is not null);

            // 2. 扫描所有带 [Component] 特性且有需要拦截方法的类
            var proxyDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (node, _) => node is ClassDeclarationSyntax cds && cds.AttributeLists.Count > 0,
                    transform: static (ctx, _) => TryGetProxyInfo(ctx)
                )
                .Where(static m => m is not null && m.Value.ProxyInfo.HasInterceptedMethods);

            // 3. 组合拦截器和代理类信息
            var combined = interceptorDeclarations.Collect()
                .Combine(proxyDeclarations.Collect())
                .Combine(context.CompilationProvider);

            // 4. 生成代理类代码
            context.RegisterSourceOutput(combined, static (spc, source) =>
            {
                var interceptors = source.Left.Left.OfType<InterceptorInfo>().ToList();
                var proxyResults = source.Left.Right
                    .Where(m => m is not null)
                    .Select(m => m!.Value)
                    .ToList();
                var compilation = source.Right;

                foreach (var result in proxyResults)
                {
                    foreach (var diag in result.Diagnostics)
                    {
                        spc.ReportDiagnostic(diag);
                    }
                }

                var proxies = proxyResults.Select(r => r.ProxyInfo).ToList();
                GenerateProxyCode(spc, compilation.AssemblyName ?? "Unknown", interceptors, proxies);
            });
        }

        private static InterceptorInfo? TryGetInterceptorInfo(GeneratorSyntaxContext context)
        {
            var symbol = context.SemanticModel.GetDeclaredSymbol(context.Node);
            if (symbol is not INamedTypeSymbol classSymbol)
            {
                return null;
            }

            // 检查是否实现 IInterceptor 接口（使用全限定名）
            var implementsInterceptor = classSymbol.AllInterfaces
                .Any(i => i.ToDisplayString() == "Csanno.Attributes.IInterceptor");

            if (!implementsInterceptor)
            {
                return null;
            }

            // 获取所有 [BindWith] 特性
            var bindings = new List<InterceptorBinding>();
            foreach (var attr in classSymbol.GetAttributes())
            {
                var attrFullName = attr.AttributeClass?.ToDisplayString();
                if (attrFullName is null)
                {
                    continue;
                }

                string? attributeType = null;
                string invokeType = "Default";

                // 处理泛型 BindWithAttribute<T>（使用全限定名前缀匹配）
                if (attrFullName.StartsWith("Csanno.Attributes.BindWithAttribute<") && attr.AttributeClass?.IsGenericType == true)
                {
                    var typeArg = attr.AttributeClass.TypeArguments.FirstOrDefault();
                    if (typeArg is not null)
                    {
                        attributeType = typeArg.ToDisplayString();
                    }
                }
                // 处理非泛型 BindWithAttribute(typeof(T))
                else if (attrFullName == "Csanno.Attributes.BindWithAttribute" && attr.ConstructorArguments.Length > 0)
                {
                    var typeArg = attr.ConstructorArguments[0].Value;
                    if (typeArg is INamedTypeSymbol namedType)
                    {
                        attributeType = namedType.ToDisplayString();
                    }
                }

                if (attributeType is null)
                {
                    continue;
                }

                // 解析 InvokeType 属性
                foreach (var namedArg in attr.NamedArguments)
                {
                    if (namedArg.Key == "InvokeType" && namedArg.Value.Value is int invokeTypeValue)
                    {
                        invokeType = invokeTypeValue switch
                        {
                            0 => "Default",
                            1 => "MustInvoke",
                            2 => "NeverInvoke",
                            3 => "WhenAllTrue",
                            4 => "WhenAnyFalse",
                            5 => "WhenAnyTrue",
                            _ => "Default"
                        };
                    }
                }

                bindings.Add(new InterceptorBinding
                {
                    AttributeType = attributeType,
                    InvokeType = invokeType
                });
            }

            if (bindings.Count == 0)
            {
                return null;
            }

            return new InterceptorInfo
            {
                FullTypeName = classSymbol.ToDisplayString(),
                ClassName = classSymbol.Name,
                Bindings = bindings
            };
        }


        private static (ProxyInfo ProxyInfo, List<Diagnostic> Diagnostics)? TryGetProxyInfo(GeneratorSyntaxContext context)
        {
            var symbol = context.SemanticModel.GetDeclaredSymbol(context.Node);
            if (symbol is not INamedTypeSymbol classSymbol)
            {
                return null;
            }

            // 排除静态类和抽象类
            if (classSymbol.IsStatic || classSymbol.IsAbstract)
            {
                return null;
            }

            // 检查是否有 [Component] 特性（含继承/派生）
            if (!SymbolAnalysis.HasComponentAttribute(classSymbol))
            {
                return null;
            }

            var diagnostics = new List<Diagnostic>();

            // 查找需要拦截的方法（带有拦截注解且是 virtual 的方法）
            // 包括本类和基类的方法
            var interceptedMethods = new List<MethodInterceptInfo>();
            var processedMethods = new HashSet<string>();

            foreach (var methodSymbol in GetAllMethods(classSymbol))
            {
                if (methodSymbol.MethodKind != MethodKind.Ordinary ||
                    !methodSymbol.IsVirtual ||
                    methodSymbol.DeclaredAccessibility != Accessibility.Public)
                {
                    continue;
                }

                var methodSignature = GetMethodSignature(methodSymbol);
                if (!processedMethods.Add(methodSignature))
                {
                    continue;
                }

                var interceptorAttributes = GetInterceptorAttributes(methodSymbol);
                if (interceptorAttributes.Count == 0)
                {
                    continue;
                }

                var location = context.Node.GetLocation();

                if (methodSymbol.IsAsync)
                {
                    diagnostics.Add(Diagnostic.Create(
                        AsyncMethodNotSupported,
                        location,
                        methodSymbol.Name));
                    continue;
                }

                if (methodSymbol.Parameters.Any(p => p.RefKind is RefKind.Ref or RefKind.Out))
                {
                    diagnostics.Add(Diagnostic.Create(
                        RefOutParameterNotSupported,
                        location,
                        methodSymbol.Name));
                    continue;
                }

                if (methodSymbol.IsGenericMethod)
                {
                    diagnostics.Add(Diagnostic.Create(
                        GenericMethodNotSupported,
                        location,
                        methodSymbol.Name));
                    continue;
                }

                interceptedMethods.Add(new MethodInterceptInfo
                {
                    MethodName = methodSymbol.Name,
                    ReturnType = methodSymbol.ReturnType.ToDisplayString(),
                    ReturnsVoid = methodSymbol.ReturnsVoid,
                    Parameters = methodSymbol.Parameters
                        .Select(p => new ProxyParameterInfo
                        {
                            Name = p.Name,
                            Type = p.Type.ToDisplayString()
                        })
                        .ToList(),
                    InterceptorAttributeTypes = interceptorAttributes
                });
            }

            if (interceptedMethods.Count == 0 && diagnostics.Count == 0)
            {
                return null;
            }

            // 获取构造函数信息
            var constructors = classSymbol.Constructors
                .Where(c => c.DeclaredAccessibility == Accessibility.Public && !c.IsStatic)
                .Select(c => new ProxyConstructorInfo
                {
                    Parameters = c.Parameters
                        .Select(p => new ProxyParameterInfo
                        {
                            Name = p.Name,
                            Type = p.Type.ToDisplayString()
                        })
                        .ToList()
                })
                .ToList();

            // 如果没有构造函数，添加默认无参构造函数
            if (constructors.Count == 0)
            {
                constructors.Add(new ProxyConstructorInfo { Parameters = new List<ProxyParameterInfo>() });
            }

            var proxyInfo = new ProxyInfo
            {
                AssemblyName = classSymbol.ContainingAssembly.Name,
                Namespace = classSymbol.ContainingNamespace.ToDisplayString(),
                ClassName = classSymbol.Name,
                FullTypeName = classSymbol.ToDisplayString(),
                InterceptedMethods = interceptedMethods,
                Constructors = constructors,
                Lifetime = SymbolAnalysis.ResolveLifetime(classSymbol, out var tags, out var ownedType),
                LifetimeScopeTags = tags,
                OwnedTypeName = ownedType,
                Services = SymbolAnalysis.ResolveServiceTypes(classSymbol),
                Metadata = SymbolAnalysis.ResolveMetadata(classSymbol)
            };

            return (proxyInfo, diagnostics);
        }

        private static IEnumerable<IMethodSymbol> GetAllMethods(INamedTypeSymbol classSymbol)
        {
            var current = classSymbol;
            while (current is not null)
            {
                foreach (var member in current.GetMembers().OfType<IMethodSymbol>())
                {
                    yield return member;
                }
                current = current.BaseType;
            }
        }

        private static string GetMethodSignature(IMethodSymbol method)
        {
            var paramTypes = string.Join(",", method.Parameters.Select(p => p.Type.ToDisplayString()));
            return $"{method.Name}({paramTypes})";
        }

        private static List<string> GetInterceptorAttributes(IMethodSymbol methodSymbol)
        {
            var result = new List<string>();

            foreach (var attr in methodSymbol.GetAttributes())
            {
                // 排除系统特性
                var attrFullName = attr.AttributeClass?.ToDisplayString();
                if (attrFullName is null)
                {
                    continue;
                }

                if (attrFullName.StartsWith("System."))
                {
                    continue;
                }

                // 添加所有用户自定义特性（可能是拦截器绑定的注解）
                result.Add(attrFullName);
            }

            return result;
        }

        private static void GenerateProxyCode(
            SourceProductionContext context,
            string assemblyName,
            List<InterceptorInfo> interceptors,
            List<ProxyInfo> proxies)
        {
            // 调试信息
            var debugInfo = new StringBuilder();
            debugInfo.AppendLine("// AOP Proxy Generator Debug Info");
            debugInfo.AppendLine($"// Interceptors found: {interceptors.Count}");
            foreach (var i in interceptors)
            {
                debugInfo.AppendLine($"//   - {i.FullTypeName} binds to: {string.Join(", ", i.BoundAttributeTypes)}");
            }
            debugInfo.AppendLine($"// Proxies to generate: {proxies.Count}");
            foreach (var p in proxies)
            {
                debugInfo.AppendLine($"//   - {p.FullTypeName} with {p.InterceptedMethods.Count} intercepted methods");
            }

            context.AddSource("AopGeneratorDebug.g.cs", debugInfo.ToString());

            if (proxies.Count == 0)
            {
                return;
            }

            // 生成代理类
            var emitter = new ProxyEmitter();
            var code = emitter.GenerateProxyClasses(assemblyName, proxies, interceptors);

            context.AddSource($"AopProxies.{assemblyName}.g.cs", code);

            // 生成代理类注册扩展方法
            GenerateProxyRegistration(context, assemblyName, proxies, interceptors);
        }

        private static void GenerateProxyRegistration(
            SourceProductionContext context,
            string assemblyName,
            List<ProxyInfo> proxies,
            List<InterceptorInfo> interceptors)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated/>");
            sb.AppendLine($"// Assembly: {assemblyName}");
            sb.AppendLine("// AOP Proxy Registration");
            sb.AppendLine("#nullable enable");
            sb.AppendLine();
            sb.AppendLine("using Autofac;");
            sb.AppendLine("using Csanno.Attributes;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine();
            sb.AppendLine("namespace Csanno.ComponentRegistration");
            sb.AppendLine("{");
            sb.AppendLine("    public static partial class AopRegistrationExtensions");
            sb.AppendLine("    {");
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// 注册所有拦截器和代理类");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        public static void RegisterAopProxies(this ContainerBuilder builder)");
            sb.AppendLine("        {");

            // 注册拦截器
            foreach (var interceptor in interceptors)
            {
                sb.AppendLine($"            // 拦截器: {interceptor.ClassName}");
                sb.AppendLine($"            builder.RegisterType<{interceptor.FullTypeName}>().As<IInterceptor>().InstancePerDependency();");
            }
            sb.AppendLine();

            // 注册代理类（替代原始类）
            foreach (var proxy in proxies)
            {
                sb.AppendLine($"            // 代理类: {proxy.ClassName}_Proxy");
                sb.Append($"            builder.RegisterType<{proxy.Namespace}.{proxy.ClassName}_Proxy>()");

                // 生命周期
                sb.Append(GetLifetimeMethod(proxy.Lifetime, proxy));

                // 服务映射
                foreach (var service in proxy.Services)
                {
                    sb.AppendLine();
                    sb.Append($"                .As<{service.ServiceType}>()");
                }

                // 元数据
                foreach (var metadata in proxy.Metadata)
                {
                    sb.AppendLine();
                    sb.Append($"                .WithMetadata(\"{metadata.Key}\", {metadata.ValueExpression})");
                }

                sb.AppendLine(";");
            }

            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            context.AddSource($"AopRegistration.{assemblyName}.g.cs", sb.ToString());
        }

        private static string GetLifetimeMethod(InstanceLifetime lifetime, ProxyInfo proxy)
        {
            return lifetime switch
            {
                InstanceLifetime.Transient => ".InstancePerDependency()",
                InstanceLifetime.Scoped => ".InstancePerLifetimeScope()",
                InstanceLifetime.Singleton => ".SingleInstance()",
                InstanceLifetime.PerRequest => ".InstancePerRequest()",
                InstanceLifetime.PerMatchingLifetimeScope =>
                    $".InstancePerMatchingLifetimeScope({string.Join(", ", proxy.LifetimeScopeTags?.Select(t => $"\"{t}\"") ?? Enumerable.Empty<string>())})",
                InstanceLifetime.Owned =>
                    $".InstancePerOwned<{proxy.OwnedTypeName ?? proxy.FullTypeName}>()",
                _ => ".InstancePerDependency()"
            };
        }

    }
}
