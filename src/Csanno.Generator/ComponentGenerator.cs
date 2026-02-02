using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Csanno.Generator.Models;
using Csanno.Generator.Emit;

namespace Csanno.Generator
{

    /// <summary>
    /// Csanno 组件注册代码生成器
    /// 使用 Roslyn Source Generator 在编译期生成 Autofac 组件注册代码
    /// </summary>
    [Generator]
    public sealed class ComponentGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // 查找带 [Component] 特性的类（包括继承自带特性基类的派生类）
            // 注意：不能只检查 AttributeLists.Count > 0，因为派生类可能没有直接特性但继承自带 [Component] 的基类
            var componentDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (node, _) => node is ClassDeclarationSyntax,
                    transform: static (ctx, _) => TryGetComponentInfo(ctx)
                )
                .Where(static m => m is not null);

            var compilationAndComponents = context.CompilationProvider
                .Combine(componentDeclarations.Collect());

            context.RegisterSourceOutput(
                compilationAndComponents,
                static (spc, source) => GenerateCode(spc, source.Right)
            );
        }

        private static ComponentInfo? TryGetComponentInfo(GeneratorSyntaxContext context)
        {
            var symbol = context.SemanticModel.GetDeclaredSymbol(context.Node);
            if (symbol is null)
            {
                return null;
            }

            // 确保是类类型
            if (symbol is not INamedTypeSymbol classSymbol)
            {
                return null;
            }

            // 排除静态类
            if (classSymbol.IsStatic)
            {
                return null;
            }

            // 排除接口
            if (classSymbol.TypeKind == TypeKind.Interface)
            {
                return null;
            }

            // 排除抽象类
            if (classSymbol.IsAbstract)
            {
                return null;
            }

            // 排除没有公共构造函数的类（Autofac 无法实例化）
            if (!HasPublicConstructor(classSymbol))
            {
                return null;
            }

            if (!SymbolAnalysis.HasComponentAttribute(classSymbol))
            {
                return null;
            }

            return ExtractComponentInfo(classSymbol);
        }

        private static bool HasPublicConstructor(INamedTypeSymbol classSymbol)
        {
            return classSymbol.Constructors.Any(c => c.DeclaredAccessibility is Accessibility.Public);
        }

        private static ComponentInfo ExtractComponentInfo(INamedTypeSymbol classSymbol)
        {
            return new ComponentInfo
            {
                AssemblyName = classSymbol.ContainingAssembly.Name,
                Namespace = classSymbol.ContainingNamespace.ToDisplayString(),
                ClassName = classSymbol.Name,
                FullTypeName = classSymbol.ToDisplayString(),
                Lifetime = SymbolAnalysis.ResolveLifetime(classSymbol, out var tags, out var ownedType),
                LifetimeScopeTags = tags,
                OwnedTypeName = ownedType,
                Services = SymbolAnalysis.ResolveServiceTypes(classSymbol),
                Metadata = SymbolAnalysis.ResolveMetadata(classSymbol)
            };
        }

        private static void GenerateCode(
            SourceProductionContext context,
            ImmutableArray<ComponentInfo?> components)
        {
            var validComponents = components.OfType<ComponentInfo>().ToList();

            if (!validComponents.Any())
            {
                return;
            }

            foreach (var group in validComponents.GroupBy(c => c.AssemblyName))
            {
                var emitter = new RegistrationEmitter();
                var code = emitter.GenerateRegistrationClass(group.Key, group);

                context.AddSource(
                    $"ComponentRegistration.{group.Key}.g.cs",
                    code);
            }
        }
    }
}
